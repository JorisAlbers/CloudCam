using System;
using OpenCvSharp;

public static class FastMatHasher
{
    private const uint FNV_OFFSET_BASIS = 2166136261;
    private const uint FNV_PRIME = 16777619;

    public static unsafe uint ComputeFastHashUnsafe(Mat mat)
    {
        if (mat.Empty())
            return 0;

        if (!mat.IsContinuous())
            throw new ArgumentException("Mat must be continuous for unsafe hash.");

        int length = (int)(mat.Total() * mat.ElemSize());

        byte* ptr = (byte*)mat.Data.ToPointer();
        uint hash = FNV_OFFSET_BASIS;

        for (int i = 0; i < length; i++)
        {
            hash ^= ptr[i];
            hash *= FNV_PRIME;
        }

        return hash;
    }
}