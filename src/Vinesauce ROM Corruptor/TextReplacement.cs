using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vinesauce_ROM_Corruptor {
    class TextReplacement {
        // Delimeter for text sections.
        static char[] Delimeter = new char[1] { '|' };
        string[] TextToReplace, ReplaceWith, Anchors;
        int[][] RelativeAnchors;
        long StartByte, EndByte;
        byte[] rom;
        // Translation dictionary.
        Dictionary<char, byte> TranslationDictionary;

        public static TextReplacement factory(string textToReplace, string replaceWith, string anchorWords, byte[] romToCorrupt) {
            return new TextReplacement(textToReplace, replaceWith, anchorWords, romToCorrupt, 0, romToCorrupt.LongLength - 1);
        }
        public TextReplacement(string textToReplace, string replaceWith, string anchorWords, byte[] romToCorrupt, long StartByte, long EndByte) {
            rom = romToCorrupt;
            // Limit the end byte.
            if(EndByte > (rom.LongLength - 1)) {
                EndByte = rom.LongLength - 1;
            }
            this.StartByte = StartByte;
            this.EndByte = EndByte;

            TranslationDictionary = new Dictionary<char, byte>();
            // Read in the text and its replacement.
            this.TextToReplace = textToReplace.Split(Delimeter, StringSplitOptions.RemoveEmptyEntries);
            this.ReplaceWith = replaceWith.Split(Delimeter, StringSplitOptions.RemoveEmptyEntries);
            // Make sure they have equal length.
            if(TextToReplace.Length != ReplaceWith.Length) {
                MainForm.ShowErrorBox("Number of text sections to replace does not match number of replacements.");
                return;
            }
            // Create relative offset arrays of the anchors.
            Anchors = anchorWords.Split(Delimeter, StringSplitOptions.RemoveEmptyEntries);
            RelativeAnchors = new int[Anchors.Length][];
            for(int i = 0; i < Anchors.Length; i++) {
                RelativeAnchors[i] = new int[Anchors[i].Length];
                for(int j = 0; j < Anchors[i].Length; j++) {
                    RelativeAnchors[i][j] = Anchors[i][j] - Anchors[i][0];
                }
            }
        }

        public byte[] Execute(List<long[]> ProtectedRegions) {
            // Look for the anchors.
            for(int i = 0; i < RelativeAnchors.Length; i++) {
                // Position in ROM.
                long j = 0;

                // Scan the entire ROM.
                while(j < rom.LongLength) {
                    // If a match has been found.
                    bool Match = true;

                    // Look for the relative values.
                    for(int k = 0; k < RelativeAnchors[i].Length; k++) {
                        // Make sure its in range.
                        if(j + k < rom.LongLength) {
                            // Ignore non-letter characters for matching purposes.
                            if(!Char.IsLetter(Anchors[i][k])) {
                                continue;
                            }

                            // Check if the relative value doesn't match.
                            if((rom[j + k] - rom[j]) != RelativeAnchors[i][k]) {
                                // It doesn't, break.
                                Match = false;
                                break;
                            }
                        } else {
                            // Out of range before matching.
                            Match = false;
                            break;
                        }
                    }

                    // If a match was found, update the dictionary.
                    if(Match) {
                        int k = 0;
                        for(k = 0; k < Anchors[i].Length; k++) {
                            if(!TranslationDictionary.ContainsKey(Anchors[i][k])) {
                                TranslationDictionary.Add(Anchors[i][k], rom[j + k]);
                            }
                        }

                        // Move ahead to the correct location in the ROM.
                        j = j + k + 1;
                    } else {
                        // Move ahead one byte.
                        j = j + 1;
                    }
                }
            }

            // Calculate the offset to translate unknown text, assuming ASCII structure.
            int ASCIIOffset = 0;
            if(TranslationDictionary.Count > 0) {
                ASCIIOffset = TranslationDictionary.First().Value - TranslationDictionary.First().Key;
            }

            // Create arrays of the text to be replaced in ROM format.
            byte[][] ByteTextToReplace = new byte[TextToReplace.Length][];
            for(int i = 0; i < TextToReplace.Length; i++) {
                ByteTextToReplace[i] = new byte[TextToReplace[i].Length];
                for(int j = 0; j < TextToReplace[i].Length; j++) {
                    if(TranslationDictionary.ContainsKey(TextToReplace[i][j])) {
                        ByteTextToReplace[i][j] = TranslationDictionary[TextToReplace[i][j]];
                    } else {
                        int ASCIITranslated = TextToReplace[i][j] + ASCIIOffset;
                        if(ASCIITranslated >= Byte.MinValue && ASCIITranslated <= Byte.MaxValue) {
                            ByteTextToReplace[i][j] = (byte)(ASCIITranslated);
                        } else {
                            // Could not translate.
                            ByteTextToReplace[i][j] = (byte)(TextToReplace[i][j]);
                        }
                    }
                }
            }

            // Create arrays of the replacement text in ROM format.
            byte[][] ByteReplaceWith = new byte[ReplaceWith.Length][];
            for(int i = 0; i < ReplaceWith.Length; i++) {
                ByteReplaceWith[i] = new byte[ReplaceWith[i].Length];
                for(int j = 0; j < ReplaceWith[i].Length; j++) {
                    if(TranslationDictionary.ContainsKey(ReplaceWith[i][j])) {
                        ByteReplaceWith[i][j] = TranslationDictionary[ReplaceWith[i][j]];
                    } else {
                        int ASCIITranslated = ReplaceWith[i][j] + ASCIIOffset;
                        if(ASCIITranslated >= Byte.MinValue && ASCIITranslated <= Byte.MaxValue) {
                            ByteReplaceWith[i][j] = (byte)(ASCIITranslated);
                        } else {
                            // Could not translate.
                            ByteReplaceWith[i][j] = (byte)(ReplaceWith[i][j]);
                        }
                    }
                }
            }

            // Area of ROM to consider.
            long TextReplacementStartByte = StartByte;
            long TextReplacementEndByte = EndByte;

            // Look for the text to replace.
            for(int i = 0; i < ByteTextToReplace.Length; i++) {
                // Position in ROM.
                long j = TextReplacementStartByte;

                // Scan the entire ROM.
                while(j <= TextReplacementEndByte) {
                    // If a match has been found.
                    bool Match = true;

                    // Look for the text.
                    for(int k = 0; k < ByteTextToReplace[i].Length; k++) {
                        // Make sure its in range.
                        if(j + k <= TextReplacementEndByte) {
                            // Ignore non-letter characters for matching purposes.
                            if(!Char.IsLetter(TextToReplace[i][k])) {
                                continue;
                            }

                            // Check if the relative value doesn't match.
                            if(rom[j + k] != ByteTextToReplace[i][k]) {
                                // It doesn't, break.
                                Match = false;
                                break;
                            }
                        } else {
                            // Out of range before matching.
                            Match = false;
                            break;
                        }
                    }

                    // If the entire string matched, replace it.
                    if(Match) {
                        // If the area is protected.
                        bool Protected = false;

                        // Length of the replacement.
                        int k = ByteReplaceWith[i].Length - 1;

                        // Check if the area is protected.
                        foreach(long[] ProtectedRegion in ProtectedRegions) {
                            if((j >= ProtectedRegion[0] && j <= ProtectedRegion[1]) || (j + k >= ProtectedRegion[0] && j + k <= ProtectedRegion[1]) || (j < ProtectedRegion[0] && j + k > ProtectedRegion[1])) {
                                // Yes, its protected.
                                Protected = true;
                                break;
                            }
                        }

                        // If not protected, replace the text.
                        if(!Protected) {
                            for(k = 0; k < ByteReplaceWith[i].Length; k++) {
                                rom[j + k] = ByteReplaceWith[i][k];
                            }

                            // Protect the inserted text.
                            ProtectedRegions.Add(new long[2] { j, j + k });
                        }

                        // Move ahead to the correct location in the ROM.
                        j = j + k + 1;
                    } else {
                        // Move ahead one byte.
                        j = j + 1;
                    }
                }
            }
            return rom;
        }
    }
}
