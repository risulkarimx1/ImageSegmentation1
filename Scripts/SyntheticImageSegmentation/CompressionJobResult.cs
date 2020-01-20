namespace AAI.VDTSimulator.ImageSegmentation
{
	public struct CompressionJobResult
	{
		public int UncompressedLength;
		public int TimeStamp;
		public byte[] CompressedData;
		public SegmentedImageType SegmentedImageType;
	}
}