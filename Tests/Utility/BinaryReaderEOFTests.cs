using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using System.IO;

namespace SvnBridge.Utility
{
    [TestFixture]
    public class BinaryReaderEOFTests
    {
        [Test]
        public void TestReadByteReadsCorrectBytes()
        {
            byte[] testData = CreateTestData(10);
            BinaryReaderEOF reader = new BinaryReaderEOF(new MemoryStream(testData));

            for (int i = 0; i < testData.Length; i++)
                Assert.AreEqual(testData[i], reader.ReadByte());
        }

        [Test]
        public void TestReadByteReadsCorrectBytesWhenReadPastBufferSize()
        {
            byte[] testData = CreateTestData(BinaryReaderEOF.BUF_SIZE + 10);
            BinaryReaderEOF reader = new BinaryReaderEOF(new MemoryStream(testData));

            for (int i = 0; i < testData.Length; i++)
                Assert.AreEqual(testData[i], reader.ReadByte());
        }

        [Test]
        public void TestReadBytesReadsCorrectBytes()
        {
            byte[] testData = CreateTestData(10);
            BinaryReaderEOF reader = new BinaryReaderEOF(new MemoryStream(testData));

            byte[] result = reader.ReadBytes(testData.Length);

            for (int i = 0; i < testData.Length; i++)
                Assert.AreEqual(testData[i], result[i]);
        }

        [Test]
        public void TestReadBytesReadsCorrectBytesWhenReadsLargerThenBufferSize()
        {
            byte[] testData = CreateTestData(BinaryReaderEOF.BUF_SIZE * 3);
            BinaryReaderEOF reader = new BinaryReaderEOF(new MemoryStream(testData));

            byte[] result = reader.ReadBytes(testData.Length);

            for (int i = 0; i < testData.Length; i++)
                Assert.AreEqual(testData[i], result[i]);
        }

        [Test]
        public void TestReadBytesReadsCorrectBytesIfReadingExactBufferSize()
        {
            byte[] testData = CreateTestData(BinaryReaderEOF.BUF_SIZE * 2);
            BinaryReaderEOF reader = new BinaryReaderEOF(new MemoryStream(testData));

            byte[] result1 = reader.ReadBytes(BinaryReaderEOF.BUF_SIZE);
            byte[] result2 = reader.ReadBytes(BinaryReaderEOF.BUF_SIZE);

            for (int i = 0; i < result1.Length; i++)
                Assert.AreEqual(testData[i], result1[i]);

            for (int i = 0; i < result2.Length; i++)
                Assert.AreEqual(testData[i + 1024], result2[i]);
        }

        [Test]
        public void TestEOFReturnsFalseIfNotEndOfStream()
        {
            byte[] testData = CreateTestData(10);
            BinaryReaderEOF reader = new BinaryReaderEOF(new MemoryStream(testData));

            bool result = reader.EOF;

            Assert.AreEqual(false, result);
        }

        [Test]
        public void TestEOFReturnsTrueIfAtEndOfStream()
        {
            byte[] testData = CreateTestData(10);
            BinaryReaderEOF reader = new BinaryReaderEOF(new MemoryStream(testData));
            reader.ReadBytes(10);

            bool result = reader.EOF;

            Assert.AreEqual(true, result);
        }

        [Test]
        public void TestEOFReturnsTrueWithZeroByteStream()
        {
            byte[] testData = CreateTestData(0);
            BinaryReaderEOF reader = new BinaryReaderEOF(new MemoryStream(testData));

            bool result = reader.EOF;

            Assert.AreEqual(true, result);
        }

        private byte[] CreateTestData(int size)
        {
            byte[] data = new byte[size];
            Random random = new Random();
            random.NextBytes(data);
            return data;
        }
    }
}
