using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ChipColor { Red, Blue, White, Black}

public class ChipType {

    public Dictionary<ChipColor, int> ChipDictionary = new Dictionary<ChipColor, int>
    {
        {ChipColor.Red, 5 },
        {ChipColor.Blue, 25 },
        {ChipColor.White, 50 },
        {ChipColor.Black, 100 }
    };

}
