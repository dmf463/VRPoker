﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Services {


    //UNIVERSAL UTITLITIES
    public static PrefabDB PrefabDB { get; set; }
    public static Dealer Dealer { get; set; }
    public static SoundManager SoundManager { get; set; }
	public static DialogueDataManager DialogueDataManager { get; set; }
    public static PokerRules PokerRules { get; set; }
    public static PlayerBehaviour PlayerBehaviour { get; set; }
    public static TextManager TextManager { get; set; }
    public static ChipManager ChipManager { get; set; }
}
