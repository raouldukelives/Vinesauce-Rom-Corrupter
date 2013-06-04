using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vinesauce_ROM_Corruptor {

    class Corruptor {

        public static void Corrupt(byte[] rom, TextReplacement textReplacementSet, ColorReplacement colorReplacementSet, ByteCorruption byteCorruption) {
            // Areas to not corrupt.
            List<long[]> ProtectedRegions = new List<long[]>();


            // Do text replacement if desired.
            if(textReplacementSet != null) {
                textReplacementSet.Execute(ProtectedRegions);
            }

            // Do color replacement if desired.
            if(colorReplacementSet != null) {
                colorReplacementSet.Execute(ProtectedRegions);
            }

            // Do byte corruption if desired.
            if(byteCorruption != null) {
                byteCorruption.Execute(ProtectedRegions);
            }
        }
    }
}
