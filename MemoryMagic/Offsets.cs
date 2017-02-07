using System;

namespace MemoryMagic
{
    public static class Offsets
    {
        // Build: 7.1.5.23420 x86
        public static IntPtr Framescript_ExecuteBuffer = new IntPtr(0x000A6D50);
        public static IntPtr ClntObjMgrGetActivePlayerObj = new IntPtr(0x00081E2D);
        public static IntPtr GameState = new IntPtr(0x00EB0B88);
        public static IntPtr FrameScript__GetLocalizedText = new IntPtr(0x002FB2BB);
        public static IntPtr PlayerNameOffset = new IntPtr(0x00F904B0);
    }
}