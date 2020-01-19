# Synthetic Image Segmentation 
Full source code of the project was not give as I worked on the porject owned by my employer.
## Project Background:
Replicar Sensor Simulator produces 3D graphics based on intelligent traffic and maps data. The simulator client renders images every frame. The purpose of this project was to segment these images so that they can be used to train Machine Learning algorithms as data set. The project produced 2D semantically segmented imgaes as well as depth images. 

## Implementation Strategy:
### Step1: Tagging the object
To create the segmented images, first, the game objects on the scene were Tagged. For game objects that may fall into the same segment and have the same parent game object, the parent game object was tagged only. SegmentedTagsManager.cs is a scriptable object which stored all the segmentable object's tag and a corresponding color.  
### Step2: Indexing MeshRenderers
From the scene, all the objects whose tags are mentioned in the SegmentedTagsManager, are looped through in SegmentedImageMaker.cs. Each segmentable object are attached with a MonoBehaviour named RenderStatus.cs whcih notifies SyntheticObjectListMaker.cs. A dictionary MeshFilterToTagsMap contains the meshRenderer and their tag.
### Step3: Segmented Image Material Generation (SegmentedImageMaker)
A material named “ObjectListMaterial” with a custom shader to hold the Object Id of each object. Object Id is stored into the red channel of a fixed4. For each mesh renderer in the dictionary, the pixel gets drawn with the incremented value of Red from this shader. The output of the shader is saved as a texture named “IndexdTexture”. A Command Buffer is filled up with the objectId. A full-screen quad creates an indexed texture. 
Another material named “SegmentedImageMaterial” with SegmentedImageShader takes 2 textures as Input. 1. TagLookUp texture contains all the colors which are assigned with the tags. Each of the pixels contains a unique color. 2. IndexedTexture: Each of the pixels of IndexedTexure contains the index of the color to be placed in the segmented image. The output of this material acts as the segmented image. 
### Step4: Saving the images
ImageSegmentationSetup.cs contains the configuration. SegmentationTimerController acts as a controller to call the functions to capture images by creating AsyncGpuReadback. AsyncImageSaver compresses the byte array coming back from the request to LzCompression format and saves to disk. 
