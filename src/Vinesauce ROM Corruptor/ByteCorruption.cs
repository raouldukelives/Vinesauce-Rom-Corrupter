using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vinesauce_ROM_Corruptor {
    class ByteCorruption {
        private List<byte> NESCPUJamProtection_Avoid = new List<byte>() { 0x48, 0x08, 0x68, 0x28, 0x78, 0x00, 0x02, 0x12, 0x22, 0x32, 0x42, 0x52, 0x62, 0x72, 0x92, 0xB2, 0xD2, 0xF2 };
        private List<byte> NESCPUJamProtection_Protect_1 = new List<byte>() { 0x48, 0x08, 0x68, 0x28, 0x78, 0x40, 0x60, 0x00, 0x90, 0xB0, 0xF0, 0x30, 0xD0, 0x10, 0x50, 0x70, 0x4C, 0x6C, 0x20 };
        private List<byte> NESCPUJamProtection_Protect_2 = new List<byte>() { 0x90, 0xB0, 0xF0, 0x30, 0xD0, 0x10, 0x50, 0x70, 0x4C, 0x6C, 0x20 };
        private List<byte> NESCPUJamProtection_Protect_3 = new List<byte>() { 0x4C, 0x6C, 0x20 };

        List<long[]> ProtectedRegions;
        byte[] rom;
        private bool shouldAdd, shouldShift, shouldReplace;
        private int AddXtoByte;
        private int ShiftRightXBytes;
        private byte ReplaceByteXwithYByteX;
        private byte ReplaceByteXwithYByteY;
        private long StartByte;
        private uint EveryNthByte;
        private long EndByte;
        private bool enableCpuJamProtection;

        public ByteCorruption(bool enableCpuJamProtection, uint EveryNthByte, byte[] romToCorrupt, long StartByte, long EndByte) {
            this.enableCpuJamProtection = enableCpuJamProtection;
            this.EveryNthByte = EveryNthByte;
            rom = romToCorrupt;
            // Limit the end byte.
            if(EndByte > (rom.LongLength - 1)) {
                EndByte = rom.LongLength - 1;
            }
            this.StartByte = StartByte;
            this.EndByte = EndByte;
        }

        public void SetAddX(int AddXtoByte){
            shouldAdd = true;
            this.AddXtoByte = AddXtoByte;
        }
        public void SetShift(int ShiftRightXBytes) {
            shouldShift = true;
            this.ShiftRightXBytes = ShiftRightXBytes;
        }
        public void SetReplace(byte ReplaceByteXwithYByteX, byte ReplaceByteXwithYByteY) {
            shouldReplace = true;
            this.ReplaceByteXwithYByteX = ReplaceByteXwithYByteX;
            this.ReplaceByteXwithYByteY = ReplaceByteXwithYByteY;
        }

        public void Execute(List<long[]> ProtectedRegions){
            this.ProtectedRegions = ProtectedRegions;

            if( shouldAdd && AddXtoByte != 0) {
                doAddX();
            } else if(shouldShift && ShiftRightXBytes != 0) {
                doShiftRight();
            } else if(shouldReplace && ReplaceByteXwithYByteX != ReplaceByteXwithYByteY) {
                doByteReplacement();
            }
        }

        void doAddX() {
            for(long i = StartByte + EveryNthByte; i <= EndByte; i = i + EveryNthByte) {
                // If the byte is protected.
                bool Protected = false;

                // Check if the byte is protected.
                foreach(long[] ProtectedRegion in ProtectedRegions) {
                    if(i >= ProtectedRegion[0] && i <= ProtectedRegion[1]) {
                        // Yes, its protected.
                        Protected = true;
                        break;
                    }
                }

                // Do NES CPU jam protection if desired.
                if(enableCpuJamProtection) {
                    if(!Protected && i >= 2) {
                        if(NESCPUJamProtection_Avoid.Contains((byte)((rom[i] + AddXtoByte) % (Byte.MaxValue + 1)))
                            || NESCPUJamProtection_Protect_1.Contains(rom[i])
                            || NESCPUJamProtection_Protect_2.Contains(rom[i - 1])
                            || NESCPUJamProtection_Protect_3.Contains(rom[i - 2])) {
                            Protected = true;
                        }
                    }
                }

                // If the byte is not protected, corrupt it.
                if(!Protected) {
                    int NewValue = (rom[i] + AddXtoByte) % (Byte.MaxValue + 1);
                    rom[i] = (byte)NewValue;
                }
            }
        }

        void doShiftRight() {
            for(long i = StartByte + EveryNthByte; i <= EndByte; i = i + EveryNthByte) {
                long j = i + ShiftRightXBytes;

                if(j >= StartByte && j <= EndByte) {
                    // If the byte is protected.
                    bool Protected = false;

                    // Check if the byte is protected.
                    foreach(long[] ProtectedRegion in ProtectedRegions) {
                        if(j >= ProtectedRegion[0] && j <= ProtectedRegion[1]) {
                            // Yes, its protected.
                            Protected = true;
                            break;
                        }
                    }

                    // Do NES CPU jam protection if desired.
                    if(enableCpuJamProtection) {
                        if(!Protected && j >= 2) {
                            if(NESCPUJamProtection_Avoid.Contains(rom[i])
                                || NESCPUJamProtection_Protect_1.Contains(rom[j])
                                || NESCPUJamProtection_Protect_2.Contains(rom[j - 1])
                                || NESCPUJamProtection_Protect_3.Contains(rom[j - 2])) {
                                Protected = true;
                            }
                        }
                    }

                    // If the byte is not protected, corrupt it.
                    if(!Protected) {
                        rom[j] = rom[i];
                    }
                }
            }
        }

        void doByteReplacement() {
            for(long i = StartByte + EveryNthByte; i <= EndByte; i = i + EveryNthByte) {
                if(rom[i] == ReplaceByteXwithYByteX) {
                    // If the byte is protected.
                    bool Protected = false;

                    // Check if the byte is protected.
                    foreach(long[] ProtectedRegion in ProtectedRegions) {
                        if(i >= ProtectedRegion[0] && i <= ProtectedRegion[1]) {
                            // Yes, its protected.
                            Protected = true;
                            break;
                        }
                    }

                    // If the byte is not protected, corrupt it.
                    if(!Protected) {
                        rom[i] = ReplaceByteXwithYByteY;
                    }
                }
            }
        }
    }
}
