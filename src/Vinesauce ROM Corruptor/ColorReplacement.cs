using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vinesauce_ROM_Corruptor {
    class ColorReplacement {
        static char[] Delimeter = new char[1] { '|' };

        public static ColorReplacement factory(string colorsToReplace, string replaceWithColors, byte[] romToCorrupt) {
            return new ColorReplacement(colorsToReplace, replaceWithColors, romToCorrupt, 0, romToCorrupt.LongLength - 1);
        }
        
        long ColorReplacementStartByte, ColorReplacementEndByte;
        string[] ColorsToReplace, ColorsReplaceWith;
        byte[] ColorsToReplaceBytes, ColorsReplaceWithBytes;
        byte[] rom;
        public ColorReplacement(string colorsToReplace, string replaceWithColors, byte[] rom, long StartByte, long EndByte) {
            this.rom = rom;

            // Read in the text and its replacement.
            ColorsToReplace = colorsToReplace.Split(Delimeter, StringSplitOptions.RemoveEmptyEntries);
            ColorsReplaceWith = replaceWithColors.Split(Delimeter, StringSplitOptions.RemoveEmptyEntries);

            // Make sure they have equal length.
            if(ColorsToReplace.Length != ColorsReplaceWith.Length) {
                MainForm.ShowErrorBox("Number of colors to replace does not match number of replacements.");
                return;
            }

            // Convert the strings.
            ColorsToReplaceBytes = new byte[ColorsToReplace.Length];
            ColorsReplaceWithBytes = new byte[ColorsReplaceWith.Length];
            for(int i = 0; i < ColorsToReplace.Length; i++) {
                try {
                    byte Converted = Convert.ToByte(ColorsToReplace[i], 16);
                    ColorsToReplaceBytes[i] = Converted;
                } catch {
                    MainForm.ShowErrorBox("Invalid color to replace.");
                    return;
                }
            }
            for(int i = 0; i < ColorsReplaceWithBytes.Length; i++) {
                try {
                    byte Converted = Convert.ToByte(ColorsReplaceWith[i], 16);
                    ColorsReplaceWithBytes[i] = Converted;
                } catch {
                    MainForm.ShowErrorBox("Invalid color replacement.");
                    return;
                }
            }
            if(EndByte > (rom.LongLength - 1)) {
                EndByte = rom.LongLength - 1;
            }
            // Area of ROM to consider.
            ColorReplacementStartByte = StartByte;
            ColorReplacementEndByte = EndByte;
        }

        public void Execute(List<long[]> ProtectedRegions){  

            // Position in ROM.
            long j = ColorReplacementStartByte;

            // Scan the entire ROM.
            while(j <= ColorReplacementEndByte) {
                // If a palette has been found.
                bool Palette = true;

                // Look for a palette.
                for(int k = 0; k < 4; k++) {
                    // Make sure its in range.
                    if(j + k <= ColorReplacementEndByte) {
                        // Check if value exceeds the maximum valid color value.
                        if(rom[j + k] > 0x3F) {
                            // It does, break.
                            Palette = false;
                            break;
                        }
                    } else {
                        // Out of range before matching.
                        Palette = false;
                        break;
                    }
                }

                // If a possible palette was found, do color replacement.
                if(Palette) {
                    for(int i = 0; i < ColorsToReplaceBytes.Length; i++) {
                        for(int k = 0; k < 4; k++) {
                            if(rom[j + k] == ColorsToReplaceBytes[i]) {
                                // If the byte is protected.
                                bool Protected = false;

                                // Check if the byte is protected.
                                foreach(long[] ProtectedRegion in ProtectedRegions) {
                                    if(j + k >= ProtectedRegion[0] && j + k <= ProtectedRegion[1]) {
                                        // Yes, its protected.
                                        Protected = true;
                                        break;
                                    }
                                }

                                // If its not protected, do the replacement.
                                if(!Protected) {
                                    rom[j + k] = ColorsReplaceWithBytes[i];
                                    ProtectedRegions.Add(new long[2] { j + k, j + k });
                                }
                            }
                        }
                    }

                    // Move ahead to the correct location in the ROM.
                    j = j + 4;
                } else {
                    // Move ahead one byte.
                    j = j + 1;
                }
            }
        }
    }
}
