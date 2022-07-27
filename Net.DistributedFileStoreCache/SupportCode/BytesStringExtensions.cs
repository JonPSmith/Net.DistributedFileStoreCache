// Copyright (c) 2022 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

namespace Net.DistributedFileStoreCache.SupportCode;

internal static class BytesStringExtensions
{
    //thanks to https://stackoverflow.com/questions/472906/how-do-i-get-a-consistent-byte-representation-of-strings-in-c-sharp-without-manu
    public static byte[] GetBytes(this string str)
    {
        byte[] bytes = new byte[str.Length * sizeof(char)];
        System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
        return bytes;
    }

    public static string GetString(this int numBytes, ref byte[] buffer)
    {
        char[] chars = new char[(numBytes / sizeof(char))];
        System.Buffer.BlockCopy(buffer, 0, chars, 0, numBytes);
        return new string(chars);
    }
}