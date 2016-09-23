using System;

namespace MemoryMagic
{
    public static class Offsets
    {
        // http://www.ownedcore.com/forums/world-of-warcraft/world-of-warcraft-bots-programs/wow-memory-editing/585582-wow-7-0-3-22624-release-info-dump-thread.html

        public static IntPtr Framescript_ExecuteBuffer = new IntPtr(0xa5bd4);       
        public static IntPtr ClntObjMgrGetActivePlayerObj = new IntPtr(0x80c25);      
        public static IntPtr GameState = new IntPtr(0x173E8CE);                      
        public static IntPtr FrameScript__GetLocalizedText = new IntPtr(0x301547);  
        public static IntPtr PlayerNameOffset = new IntPtr(0xF3B288);               
    }
}