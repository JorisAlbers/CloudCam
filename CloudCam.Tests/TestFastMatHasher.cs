using NUnit.Framework;
using OpenCvSharp;

namespace CloudCam.Tests
{
   

    [TestFixture]
    public class FastMatHasherTests
    {
        [Test]
        public void ComputeFastHashUnsafe_ShouldReturnSameHash_ForIdenticalMats()
        {
            using var mat1 = new Mat(100, 100, MatType.CV_8UC1, Scalar.All(42));
            using var mat2 = mat1.Clone(); // identical content

            uint hash1 = FastMatHasher.ComputeFastHashUnsafe(mat1);
            uint hash2 = FastMatHasher.ComputeFastHashUnsafe(mat2);

            Assert.AreEqual(hash1, hash2, "Hashes should match for identical Mats");
        }

        [Test]
        public void ComputeFastHashUnsafe_ShouldReturnDifferentHash_WhenMatChanges()
        {
            using var mat = new Mat(50, 50, MatType.CV_8UC1, Scalar.All(0));
            uint originalHash = FastMatHasher.ComputeFastHashUnsafe(mat);

            // Modify a pixel
            mat.Set<byte>(10, 10, 255);

            uint newHash = FastMatHasher.ComputeFastHashUnsafe(mat);

            Assert.AreNotEqual(originalHash, newHash, "Hash should change when Mat content changes");
        }

        [Test]
        public void ComputeFastHashUnsafe_ShouldReturnZero_ForEmptyMat()
        {
            using var emptyMat = new Mat();
            uint hash = FastMatHasher.ComputeFastHashUnsafe(emptyMat);

            Assert.AreEqual(0, hash, "Empty mat should return 0 hash");
        }

        [Test]
        public void ComputeFastHashUnsafe_ShouldThrow_ForNonContinuousMat()
        {
            // Create a non-continuous Mat by taking a ROI
            using var baseMat = new Mat(10, 10, MatType.CV_8UC1, Scalar.All(1));
            var roi = new Mat(baseMat, new Rect(1, 1, 5, 5)); // ROI is usually non-continuous

            Assert.IsFalse(roi.IsContinuous());

            Assert.Throws<System.ArgumentException>(() =>
            {
                FastMatHasher.ComputeFastHashUnsafe(roi);
            }, "Non-continuous Mat should throw an exception");
        }
    }

}
