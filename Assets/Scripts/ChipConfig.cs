using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ChipColor { Red, Blue, White, Black}

//this is the class that holds the chipValues, so that if we want to add or change the values we don't need to adjust EVERY SINGLE INSTANCE WHERE WE MENTION THE VALUE, we just change it here

public class ChipConfig {

    public const int RED_CHIP_VALUE = 25;
    public const int BLUE_CHIP_VALUE = 50;
    public const int WHITE_CHIP_VALUE = 100;
    public const int BLACK_CHIP_VALUE = 500;

}
