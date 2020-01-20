using Unity.Collections;
using Unity.Jobs;

namespace AAI.VDTSimulator.ImageSegmentation
{
	public struct CompressionJob
	{
		[ReadOnly] public NativeArray<byte> InputBuffer;
		public int UncompressedLength;
		public int TimeStamp;

		public NativeArray<byte> CompressedData;
		public NativeArray<int> CompressedLength;

		public JobHandle JobHandle;
		public SegmentedImageType SegmentedImageType;
	}
}