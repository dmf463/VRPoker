using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChipManager {

    public List<Vector3> chipPositionWhenPushing = new List<Vector3>();
    public int chipsBeingPushed;
    public List<Chip> chipGroup= new List<Chip>();
    public List<Vector3> chipPositionInPot;

    public void ChipInit()
    {
        chipPositionInPot = Services.ChipManager.CreateChipPositionsForPot(GameObject.Find("TipZone").transform.position, 0.075f, 0.06f, 5, 50, GameObject.Find("TipZone").transform.position.y);
    }

    public void ChipUpdate()
    {
        PushGroupOfChips();
    }

    public List<GameObject> chipsToDestroy = new List<GameObject>();

    public void DestroyChips()
    {
        if (chipsToDestroy.Count > 0)
        {
            foreach (GameObject chip in chipsToDestroy)
            {
                GameObject.Destroy(chip);
            }
        }
    }
    public void PushGroupOfChips()
    {
        //chipPositionWhenPushing = CreateChipPositionsForPushing(chipGroup[0].transform.position, 0.06f, 0.075f, 5, 25);
        Vector3 offset = new Vector3(0.1f, 0, 0);
        if (chipGroup.Count != 0)
        {
            for (int i = 0; i < chipGroup.Count; i++)
            {
                Rigidbody rb = chipGroup[i].gameObject.GetComponent<Rigidbody>();
                int chipSpotIndex = chipGroup[i].spotIndex;
                float scalar = 1.1f;
                Vector3 startPos = chipGroup[i].chipPushStartPos + offset;
                Vector3 stickPos = GameObject.FindGameObjectWithTag("StickHead").transform.position;
                Vector3 linearDest = (stickPos - startPos) * scalar;
                Vector3 dest = GameObject.FindGameObjectWithTag("StickHead").transform.position;
                if (rb != null)
                {
                    Debug.Log("should be moving chips");
                    rb.MovePosition(new Vector3(dest.x + linearDest.x, chipGroup[i].gameObject.transform.position.y, dest.z + linearDest.z));
                }
            }
        }
    }

    public List<int> SetChipStacks(int chipAmount)
    {

        List<int> startingStack = new List<int>();

        List<GameObject> playerPositions = new List<GameObject>
        {
            GameObject.Find("P0Chips"), GameObject.Find("P1Chips"), GameObject.Find("P2Chips"), GameObject.Find("P3Chips"), GameObject.Find("P4Chips")
        };

        int valueRemaining = chipAmount;
        int blackChipCount = 0;
        int whiteChipCount = 0;
        int blueChipCount = 0;
        int redChipCount = 0;

        //change these hard coded variables to a function that finds the proper amount of chips based on a percent of the chipAmount
        int blackChipCountMAX = (int)((chipAmount * 0.45f) / ChipConfig.BLACK_CHIP_VALUE);
        int whiteChipCountMAX = (int)((chipAmount * 0.35f) / ChipConfig.WHITE_CHIP_VALUE);
        int blueChipCountMAX = (int)((chipAmount) * 0.15f / ChipConfig.BLUE_CHIP_VALUE);

        blackChipCount = Mathf.Min(blackChipCountMAX, valueRemaining / ChipConfig.BLACK_CHIP_VALUE);
        valueRemaining -= (blackChipCount * ChipConfig.BLACK_CHIP_VALUE);
        startingStack.Add(blackChipCount);

        whiteChipCount = Mathf.Min(whiteChipCountMAX, valueRemaining / ChipConfig.WHITE_CHIP_VALUE);
        valueRemaining -= (whiteChipCount * ChipConfig.WHITE_CHIP_VALUE);
        startingStack.Add(whiteChipCount);

        blueChipCount = Mathf.Min(blueChipCountMAX, valueRemaining / ChipConfig.BLUE_CHIP_VALUE);
        valueRemaining -= (blueChipCount * ChipConfig.BLUE_CHIP_VALUE);
        startingStack.Add(blueChipCount);

        redChipCount = valueRemaining / ChipConfig.RED_CHIP_VALUE;
        startingStack.Add(redChipCount);

        return startingStack;
    }

    public void CreateAndOrganizeChipStacks(List<int> chipsToOrganize, List<GameObject> parentChips, int SeatPos)
    {
        GameObject[] emptyContainers = GameObject.FindGameObjectsWithTag("Container");
        foreach (GameObject container in emptyContainers)
        {
            if (container.transform.childCount == 0)
            {
                GameObject.Destroy(container);
            }
        }
        if (parentChips.Count != 0)
        {
            foreach (GameObject chip in parentChips)
            {
                //Destroy(chip);
                Services.ChipManager.chipsToDestroy.Add(chip);
            }
            parentChips.Clear();
        }

        List<int> organizedChips = chipsToOrganize;
        GameObject parentChip = null;
        float incrementStackBy = 0;
        List<GameObject> playerPositions = new List<GameObject>
        {
            GameObject.Find("P0Chips"), GameObject.Find("P1Chips"), GameObject.Find("P2Chips"), GameObject.Find("P3Chips"), GameObject.Find("P4Chips")
        };
        Vector3 offSet = Vector3.zero;
        Vector3 containerOffset = Vector3.up * 0f; //.08
        GameObject chipContainer = GameObject.Instantiate(new GameObject(), playerPositions[SeatPos].transform.position + containerOffset, playerPositions[SeatPos].transform.rotation);
        chipContainer.tag = "Container";
        chipContainer.name = "Container";
        chipContainer.transform.rotation = Quaternion.Euler(0, chipContainer.transform.rotation.eulerAngles.y + 90, 0);
        Vector3 lastStackPos = Vector3.zero;
        Vector3 firstStackPos = Vector3.zero;

        int stackCountMax = 30;
        int stacksCreated = 0;
        //int stackRowMax = 5;

        for (int chipStacks = 0; chipStacks < organizedChips.Count; chipStacks++)
        {
            GameObject chipToMake = null;
            if (organizedChips[chipStacks] != 0)
            {
                switch (chipStacks)
                {
                    case 0:
                        chipToMake = FindChipPrefab(ChipConfig.BLACK_CHIP_VALUE);
                        chipToMake.GetComponent<Chip>().chipData = new ChipData(ChipConfig.BLACK_CHIP_VALUE);
                        break;
                    case 1:
                        chipToMake = FindChipPrefab(ChipConfig.WHITE_CHIP_VALUE);
                        chipToMake.GetComponent<Chip>().chipData = new ChipData(ChipConfig.WHITE_CHIP_VALUE);
                        break;
                    case 2:
                        chipToMake = FindChipPrefab(ChipConfig.BLUE_CHIP_VALUE);
                        chipToMake.GetComponent<Chip>().chipData = new ChipData(ChipConfig.BLUE_CHIP_VALUE);
                        break;
                    case 3:
                        chipToMake = FindChipPrefab(ChipConfig.RED_CHIP_VALUE);
                        chipToMake.GetComponent<Chip>().chipData = new ChipData(ChipConfig.RED_CHIP_VALUE);
                        break;
                    default:
                        break;
                }
                int chipStackSize = 0;
                for (int chipIndex = 0; chipIndex < organizedChips[chipStacks]; chipIndex++)
                {
                    if (chipIndex == 0)
                    {
                        chipStackSize++;
                        stacksCreated++;
                        //if(stacksCreated >= stackRowMax)
                        //{
                        //    stacksCreated = 0;
                        //    offSet += new Vector3(parentChip.GetComponent<Collider>().bounds.size.z + .01f, 0, 0);
                        //}
                        //Debug.Log("ChipToMake = " + chipToMake);
                        parentChip = GameObject.Instantiate(chipToMake, chipContainer.transform.position, Quaternion.identity) as GameObject;
                        parentChip.GetComponent<Chip>().owner = Services.Dealer.players[SeatPos];
                        parentChip.GetComponent<Chip>().chipData = new ChipData(chipToMake.GetComponent<Chip>().chipData.ChipValue);
                        parentChips.Add(parentChip);
                        parentChip.transform.parent = chipContainer.transform;
                        parentChip.transform.rotation = Quaternion.Euler(-90, 0, 0);
                        parentChip.GetComponent<Chip>().chipStack = new ChipStack(parentChip.GetComponent<Chip>());
                        if (parentChip.GetComponent<Rigidbody>() == null)
                        {
                            parentChip.AddComponent<Rigidbody>();
                        }
                        //incrementStackBy = parentChip.gameObject.GetComponent<Collider>().bounds.size.y;
                        incrementStackBy = parentChip.transform.localScale.z;
                        parentChip.transform.localPosition = offSet;
                        offSet += new Vector3(parentChip.GetComponent<Collider>().bounds.size.x + .01f, 0, 0);
                        if (firstStackPos == Vector3.zero)
                        {
                            firstStackPos = parentChip.transform.position;
                        }
                        lastStackPos = parentChip.transform.position;
                    }
                    else if (chipStackSize >= stackCountMax)
                    {
                        //Debug.Log("creating new stack cause max stack count reached");
                        chipStackSize = 0;
                        stacksCreated++;
                        //if (stacksCreated >= stackRowMax)
                        //{
                        //    Debug.Log("moving row forward");
                        //    stacksCreated = 0;
                        //    offSet += new Vector3(0, 0, (parentChip.GetComponent<Collider>().bounds.size.x + .01f) * 1.5f);
                        //}
                        //parentChip = organizedChips[chipStacks][chipIndex];
                        parentChip = GameObject.Instantiate(chipToMake, chipContainer.transform.position, Quaternion.identity) as GameObject;
                        parentChip.GetComponent<Chip>().chipData = new ChipData(chipToMake.GetComponent<Chip>().chipData.ChipValue);
                        parentChip.GetComponent<Chip>().owner = Services.Dealer.players[SeatPos];
                        parentChips.Add(parentChip);
                        parentChip.transform.parent = chipContainer.transform;
                        parentChip.transform.rotation = Quaternion.Euler(-90, 0, 0);
                        parentChip.GetComponent<Chip>().chipStack = new ChipStack(parentChip.GetComponent<Chip>());
                        if (parentChip.GetComponent<Rigidbody>() == null)
                        {
                            parentChip.AddComponent<Rigidbody>();
                        }
                        //incrementStackBy = parentChip.gameObject.GetComponent<Collider>().bounds.size.y;
                        incrementStackBy = parentChip.transform.localScale.z;
                        parentChip.transform.localPosition = offSet;
                        offSet += new Vector3(parentChip.GetComponent<Collider>().bounds.size.x + .01f, 0, 0);
                        if (firstStackPos == Vector3.zero)
                        {
                            firstStackPos = parentChip.transform.position;
                        }
                        lastStackPos = parentChip.transform.position;
                    }
                    else
                    {
                        chipStackSize++;
                        parentChip.transform.localScale = new Vector3(parentChip.transform.localScale.x,
                                                                      parentChip.transform.localScale.y,
                                                                      parentChip.transform.localScale.z + incrementStackBy);
                        ChipData newChipData = new ChipData(chipToMake.GetComponent<Chip>().chipData.ChipValue);
                        parentChip.GetComponent<Chip>().chipStack.chips.Add(newChipData);
                        parentChip.GetComponent<Chip>().chipStack.stackValue += newChipData.ChipValue;
                    }
                }
            }
        }
        Vector3 trueOffset = firstStackPos - lastStackPos;
        chipContainer.transform.position += trueOffset / 2;
    }

    public void Bet(int betAmount, bool isTipping, int SeatPos, int chipCount, List<GameObject> parentChips)
    {
        if (betAmount != 0) Services.TextManager.ShowBetAmount(SeatPos, betAmount);
        if (isTipping) Services.SoundManager.GenerateSourceAndPlay(Services.SoundManager.tipSFX, 1f);
        else Services.SoundManager.GenerateSourceAndPlay(Services.SoundManager.chips[Random.Range(0, Services.SoundManager.chips.Length)], 0.25f, Random.Range(0.95f, 1.05f), Services.Dealer.players[SeatPos].transform.position);
        int oldChipStackValue = chipCount;
        List<GameObject> playerBetZones = new List<GameObject>
        {
            GameObject.Find("P0BetZone"), GameObject.Find("P1BetZone"), GameObject.Find("P2BetZone"), GameObject.Find("P3BetZone"), GameObject.Find("P4BetZone")
        };
        Vector3 tipPos = GameObject.Find("TipZone").transform.position;
        List<GameObject> betChips = new List<GameObject>();
        List<int> colorChipCount = new List<int>()
        {
			//blackChipCount, whiteChipCount, blueChipCount, redChipCount
			0, 0, 0, 0
        };

        List<int> chipPrefab = new List<int>
        {
            ChipConfig.BLACK_CHIP_VALUE, ChipConfig.WHITE_CHIP_VALUE, ChipConfig.BLUE_CHIP_VALUE, ChipConfig.RED_CHIP_VALUE
        };

        int valueRemaining = betAmount;
        int remainder = valueRemaining % ChipConfig.RED_CHIP_VALUE;
        if (remainder > 0)
        {
            valueRemaining = (valueRemaining - remainder) + ChipConfig.RED_CHIP_VALUE;
        }

        int blackChipCountMAX = FindChipMax(ChipConfig.BLACK_CHIP_VALUE, Services.Dealer.players[SeatPos]);
        int whiteChipCountMAX = FindChipMax(ChipConfig.WHITE_CHIP_VALUE, Services.Dealer.players[SeatPos]);
        int blueChipCountMAX = FindChipMax(ChipConfig.BLUE_CHIP_VALUE, Services.Dealer.players[SeatPos]);
        int redChipCountMAX = FindChipMax(ChipConfig.RED_CHIP_VALUE, Services.Dealer.players[SeatPos]);

        colorChipCount[0] = Mathf.Min(blackChipCountMAX, valueRemaining / ChipConfig.BLACK_CHIP_VALUE);
        valueRemaining -= colorChipCount[0] * ChipConfig.BLACK_CHIP_VALUE;

        colorChipCount[1] = Mathf.Min(whiteChipCountMAX, valueRemaining / ChipConfig.WHITE_CHIP_VALUE);
        valueRemaining -= colorChipCount[1] * ChipConfig.WHITE_CHIP_VALUE;

        colorChipCount[2] = Mathf.Min(blueChipCountMAX, valueRemaining / ChipConfig.BLUE_CHIP_VALUE);
        valueRemaining -= colorChipCount[2] * ChipConfig.BLUE_CHIP_VALUE;

        colorChipCount[3] = Mathf.Min(redChipCountMAX, valueRemaining / ChipConfig.RED_CHIP_VALUE);
        valueRemaining -= colorChipCount[3] * ChipConfig.RED_CHIP_VALUE;

        if (valueRemaining > 0)
        {
            Debug.Log("value Remaining > 0");
            List<int> chipChange = SetChipStacks(valueRemaining);
            for (int i = 0; i < chipChange.Count; i++)
            {
                for (int chipChangeIndex = 0; chipChangeIndex < chipChange[chipChangeIndex]; chipChangeIndex++)
                {
                    if (chipChange[chipChangeIndex] != 0) colorChipCount[i]++;
                }
            }
            Table.instance.RemoveChipFrom(Table.instance.playerDestinations[SeatPos], valueRemaining);
        }
        GameObject parentChip = null;
        float incrementStackBy = 0;
        Vector3 offSet = Vector3.zero;
        Vector3 containerOffset = Vector3.up * .08f;
        GameObject chipContainer;
        if (!isTipping)
        {
            chipContainer = GameObject.Instantiate(new GameObject(), playerBetZones[SeatPos].transform.position + containerOffset, playerBetZones[SeatPos].transform.rotation);
        }
        else
        {
            chipContainer = GameObject.Instantiate(new GameObject(), playerBetZones[SeatPos].transform.position + containerOffset, playerBetZones[SeatPos].transform.rotation);
        }
        if (!isTipping) chipContainer.tag = "TipContainer";
        else chipContainer.tag = "Container";
        chipContainer.name = "Container";
        chipContainer.transform.rotation = Quaternion.Euler(0, chipContainer.transform.rotation.eulerAngles.y + 90, 0);
        Vector3 lastStackPos = Vector3.zero;
        Vector3 firstStackPos = Vector3.zero;
        int chipCountMax = 30;
        for (int colorListIndex = 0; colorListIndex < colorChipCount.Count; colorListIndex++) //this runs 4 times, one for each color
        {
            GameObject chipToMake = null;
            switch (colorListIndex)
            {
                case 0:
                    chipToMake = FindChipPrefab(ChipConfig.BLACK_CHIP_VALUE);
                    chipToMake.GetComponent<Chip>().chipData = new ChipData(ChipConfig.BLACK_CHIP_VALUE);
                    break;
                case 1:
                    chipToMake = FindChipPrefab(ChipConfig.WHITE_CHIP_VALUE);
                    chipToMake.GetComponent<Chip>().chipData = new ChipData(ChipConfig.WHITE_CHIP_VALUE);
                    break;
                case 2:
                    chipToMake = FindChipPrefab(ChipConfig.BLUE_CHIP_VALUE);
                    chipToMake.GetComponent<Chip>().chipData = new ChipData(ChipConfig.BLUE_CHIP_VALUE);
                    break;
                case 3:
                    chipToMake = FindChipPrefab(ChipConfig.RED_CHIP_VALUE);
                    chipToMake.GetComponent<Chip>().chipData = new ChipData(ChipConfig.RED_CHIP_VALUE);
                    break;
                default:
                    break;
            }
            if (colorChipCount.Count != 0) //if there is a number
            {
                int chipStackCount = 0;
                for (int chipIndex = 0; chipIndex < colorChipCount[colorListIndex]; chipIndex++)
                {
                    if (chipIndex == 0)
                    {
                        chipStackCount++;
                        GameObject newChip;
                        if (!isTipping)
                        {
                            newChip = GameObject.Instantiate(chipToMake, playerBetZones[SeatPos].transform.position + offSet, Quaternion.Euler(-90, 0, 0));
                        }
                        else
                        {
                            newChip = GameObject.Instantiate(chipToMake, playerBetZones[SeatPos].transform.position + offSet, Quaternion.Euler(-90, 0, 0));
                            newChip.GetComponent<MeshRenderer>().material = Services.PokerRules.tipMaterial;
                            newChip.gameObject.tag = "Tip";
                            Chip _chip = newChip.GetComponent<Chip>();
                            newChip.GetComponent<Rigidbody>().velocity = _chip.BallisticVel(_chip.myTarget, _chip.flyTime);
                        }
                        newChip.GetComponent<Chip>().chipData = new ChipData(chipToMake.GetComponent<Chip>().chipData.ChipValue);
                        betChips.Add(newChip);
                        Table.instance.RemoveChipFrom(Table.instance.playerDestinations[SeatPos], newChip.GetComponent<Chip>().chipData.ChipValue);
                        if (!isTipping) Table.instance.potChips += newChip.GetComponent<Chip>().chipData.ChipValue;
                        parentChip = newChip;
                        if (!isTipping) Services.Dealer.chipsInPot.Add(newChip.GetComponent<Chip>());
                        parentChip.transform.parent = chipContainer.transform;
                        parentChip.transform.rotation = Quaternion.Euler(-90, 0, 0);
                        parentChip.GetComponent<Chip>().chipStack = new ChipStack(parentChip.GetComponent<Chip>());
                        if (parentChip.GetComponent<Rigidbody>() == null)
                        {
                            parentChip.AddComponent<Rigidbody>();
                        }
                        incrementStackBy = parentChip.transform.localScale.z;
                        parentChip.transform.localPosition = offSet;
                        offSet += new Vector3(parentChip.GetComponent<Collider>().bounds.size.x + .01f, 0, 0);
                        if (firstStackPos == Vector3.zero)
                        {
                            firstStackPos = parentChip.transform.position;
                        }
                        lastStackPos = parentChip.transform.position;
                    }
                    else if (chipStackCount >= chipCountMax)
                    {
                        chipStackCount = 1;
                        GameObject newChip;
                        if (!isTipping)
                        {
                            newChip = GameObject.Instantiate(chipToMake, playerBetZones[SeatPos].transform.position + offSet, Quaternion.Euler(-90, 0, 0));
                        }
                        else
                        {
                            newChip = GameObject.Instantiate(chipToMake, playerBetZones[SeatPos].transform.position + offSet, Quaternion.Euler(-90, 0, 0));
                            newChip.GetComponent<MeshRenderer>().material = Services.PokerRules.tipMaterial;
                            newChip.gameObject.tag = "Tip";
                            Chip _chip = newChip.GetComponent<Chip>();
                            newChip.GetComponent<Rigidbody>().velocity = _chip.BallisticVel(_chip.myTarget, _chip.flyTime);
                        }
                        newChip.GetComponent<Chip>().chipData = new ChipData(chipToMake.GetComponent<Chip>().chipData.ChipValue);
                        betChips.Add(newChip);
                        Table.instance.RemoveChipFrom(Table.instance.playerDestinations[SeatPos], newChip.GetComponent<Chip>().chipData.ChipValue);
                        if (!isTipping) Table.instance.potChips += newChip.GetComponent<Chip>().chipData.ChipValue;
                        parentChip = newChip;
                        if (!isTipping) Services.Dealer.chipsInPot.Add(newChip.GetComponent<Chip>());
                        parentChip.transform.parent = chipContainer.transform;
                        parentChip.transform.rotation = Quaternion.Euler(-90, 0, 0);
                        parentChip.GetComponent<Chip>().chipStack = new ChipStack(parentChip.GetComponent<Chip>());
                        if (parentChip.GetComponent<Rigidbody>() == null)
                        {
                            parentChip.AddComponent<Rigidbody>();
                        }
                        incrementStackBy = parentChip.transform.localScale.z;
                        parentChip.transform.localPosition = offSet;
                        offSet += new Vector3(parentChip.GetComponent<Collider>().bounds.size.x + .01f, 0, 0);
                        if (firstStackPos == Vector3.zero)
                        {
                            firstStackPos = parentChip.transform.position;
                        }
                        lastStackPos = parentChip.transform.position;
                    }
                    else
                    {
                        chipStackCount++;
                        ChipData newChipData = new ChipData(chipToMake.GetComponent<Chip>().chipData.ChipValue);
                        Table.instance.RemoveChipFrom(Table.instance.playerDestinations[SeatPos], newChipData.ChipValue);
                        if (!isTipping) Table.instance.potChips += newChipData.ChipValue;
                        parentChip.GetComponent<Chip>().chipStack.chips.Add(newChipData);
                        parentChip.GetComponent<Chip>().chipStack.stackValue += newChipData.ChipValue;
                        parentChip.transform.localScale = new Vector3(parentChip.transform.localScale.x,
                                                                      parentChip.transform.localScale.y,
                                                                      parentChip.transform.localScale.z + incrementStackBy);
                    }
                }
            }
        }
        Vector3 trueOffset = firstStackPos - lastStackPos;
        chipContainer.transform.position += trueOffset / 2;

        List<int> newChipStack = SetChipStacks(Services.Dealer.players[SeatPos].chipCount);
        CreateAndOrganizeChipStacks(newChipStack, parentChips, SeatPos);
        GameObject[] emptyContainers = GameObject.FindGameObjectsWithTag("Container");
        foreach (GameObject container in emptyContainers)
        {
            if (container.transform.childCount == 0)
            {
                GameObject.Destroy(container);
            }
            //else Debug.Log("container has " + container.transform.childCount + " children");
        }
        foreach (GameObject chip in betChips)
        {
            chip.GetComponent<Chip>().chipForBet = true;
        }
    }

    public List<int> PrepChipsForSplit(int chipAmount)
    {

        List<int> startingStack = new List<int>();

        List<GameObject> playerPositions = new List<GameObject>
        {
            GameObject.Find("P0Cards"), GameObject.Find("P1Cards"), GameObject.Find("P2Cards"), GameObject.Find("P3Cards"), GameObject.Find("P4Cards")
        };

        int valueRemaining = chipAmount;
        int blackChipCount = 0;
        int whiteChipCount = 0;
        int blueChipCount = 0;
        int redChipCount = 0;

        //change these hard coded variables to a function that finds the proper amount of chips based on a percent of the chipAmount
        int blackChipCountMAX = (int)((chipAmount * 0.45f) / ChipConfig.BLACK_CHIP_VALUE);
        int whiteChipCountMAX = (int)((chipAmount * 0.35f) / ChipConfig.WHITE_CHIP_VALUE);
        int blueChipCountMAX = (int)((chipAmount) * 0.15f / ChipConfig.BLUE_CHIP_VALUE);

        blackChipCount = Mathf.Min(blackChipCountMAX, valueRemaining / ChipConfig.BLACK_CHIP_VALUE);
        valueRemaining -= (blackChipCount * ChipConfig.BLACK_CHIP_VALUE);
        startingStack.Add(blackChipCount);

        whiteChipCount = Mathf.Min(whiteChipCountMAX, valueRemaining / ChipConfig.WHITE_CHIP_VALUE);
        valueRemaining -= (whiteChipCount * ChipConfig.WHITE_CHIP_VALUE);
        startingStack.Add(whiteChipCount);

        blueChipCount = Mathf.Min(blueChipCountMAX, valueRemaining / ChipConfig.BLUE_CHIP_VALUE);
        valueRemaining -= (blueChipCount * ChipConfig.BLUE_CHIP_VALUE);
        startingStack.Add(blueChipCount);

        redChipCount = valueRemaining / ChipConfig.RED_CHIP_VALUE;
        startingStack.Add(redChipCount);

        return startingStack;
    }

    public void SplitPot(List<int> chipsToOrganize, int SeatPos, List<Chip> chipsInPot)
    {
        GameObject[] emptyContainers = GameObject.FindGameObjectsWithTag("Container");
        foreach (GameObject container in emptyContainers)
        {
            if (container.transform.childCount == 0)
            {
                GameObject.Destroy(container);
            }
        }

        List<int> organizedChips = chipsToOrganize;
        GameObject parentChip = null;
        float incrementStackBy = 0;
        List<GameObject> playerPositions = new List<GameObject>
        {
            GameObject.Find("P0BetZone"), GameObject.Find("P1BetZone"), GameObject.Find("P2BetZone"), GameObject.Find("P3BetZone"), GameObject.Find("P4BetZone")
        };
        Vector3 offSet = Vector3.zero;
        Vector3 containerOffset = Vector3.up * .08f;
        GameObject chipContainer = GameObject.Instantiate(new GameObject(), playerPositions[SeatPos].transform.position + containerOffset, playerPositions[SeatPos].transform.rotation);
        chipContainer.tag = "Container";
        chipContainer.name = "Container";
        chipContainer.transform.rotation = Quaternion.Euler(0, chipContainer.transform.rotation.eulerAngles.y + 90, 0);
        Vector3 lastStackPos = Vector3.zero;
        Vector3 firstStackPos = Vector3.zero;

        int stackCountMax = 30;
        int stacksCreated = 0;
        //int stackRowMax = 5;

        for (int chipStacks = 0; chipStacks < organizedChips.Count; chipStacks++)
        {
            GameObject chipToMake = null;
            if (organizedChips[chipStacks] != 0)
            {
                switch (chipStacks)
                {
                    case 0:
                        chipToMake = FindChipPrefab(ChipConfig.BLACK_CHIP_VALUE);
                        chipToMake.GetComponent<Chip>().chipData = new ChipData(ChipConfig.BLACK_CHIP_VALUE);
                        break;
                    case 1:
                        chipToMake = FindChipPrefab(ChipConfig.WHITE_CHIP_VALUE);
                        chipToMake.GetComponent<Chip>().chipData = new ChipData(ChipConfig.WHITE_CHIP_VALUE);
                        break;
                    case 2:
                        chipToMake = FindChipPrefab(ChipConfig.BLUE_CHIP_VALUE);
                        chipToMake.GetComponent<Chip>().chipData = new ChipData(ChipConfig.BLUE_CHIP_VALUE);
                        break;
                    case 3:
                        chipToMake = FindChipPrefab(ChipConfig.RED_CHIP_VALUE);
                        chipToMake.GetComponent<Chip>().chipData = new ChipData(ChipConfig.RED_CHIP_VALUE);
                        break;
                    default:
                        break;
                }
                int chipStackSize = 0;
                for (int chipIndex = 0; chipIndex < organizedChips[chipStacks]; chipIndex++)
                {
                    if (chipIndex == 0)
                    {
                        chipStackSize++;
                        stacksCreated++;
                        parentChip = GameObject.Instantiate(chipToMake, chipContainer.transform.position, Quaternion.identity) as GameObject;
                        chipsInPot.Add(parentChip.GetComponent<Chip>());
                        parentChip.GetComponent<Chip>().chipData = new ChipData(chipToMake.GetComponent<Chip>().chipData.ChipValue);
                        parentChip.transform.parent = chipContainer.transform;
                        parentChip.transform.rotation = Quaternion.Euler(-90, 0, 0);
                        parentChip.GetComponent<Chip>().chipStack = new ChipStack(parentChip.GetComponent<Chip>());
                        if (parentChip.GetComponent<Rigidbody>() == null)
                        {
                            parentChip.AddComponent<Rigidbody>();
                        }
                        incrementStackBy = parentChip.transform.localScale.z;
                        parentChip.transform.localPosition = offSet;
                        offSet += new Vector3(parentChip.GetComponent<Collider>().bounds.size.x + .01f, 0, 0);
                        if (firstStackPos == Vector3.zero)
                        {
                            firstStackPos = parentChip.transform.position;
                        }
                        lastStackPos = parentChip.transform.position;
                    }
                    else if (chipStackSize >= stackCountMax)
                    {
                        chipStackSize = 0;
                        stacksCreated++;
                        parentChip = GameObject.Instantiate(chipToMake, chipContainer.transform.position, Quaternion.identity) as GameObject;
                        chipsInPot.Add(parentChip.GetComponent<Chip>());
                        parentChip.GetComponent<Chip>().chipData = new ChipData(chipToMake.GetComponent<Chip>().chipData.ChipValue);
                        parentChip.transform.parent = chipContainer.transform;
                        parentChip.transform.rotation = Quaternion.Euler(-90, 0, 0);
                        parentChip.GetComponent<Chip>().chipStack = new ChipStack(parentChip.GetComponent<Chip>());
                        if (parentChip.GetComponent<Rigidbody>() == null)
                        {
                            parentChip.AddComponent<Rigidbody>();
                        }
                        incrementStackBy = parentChip.transform.localScale.z;
                        parentChip.transform.localPosition = offSet;
                        offSet += new Vector3(parentChip.GetComponent<Collider>().bounds.size.x + .01f, 0, 0);
                        if (firstStackPos == Vector3.zero)
                        {
                            firstStackPos = parentChip.transform.position;
                        }
                        lastStackPos = parentChip.transform.position;
                    }
                    else
                    {
                        chipStackSize++;
                        parentChip.transform.localScale = new Vector3(parentChip.transform.localScale.x,
                                                                      parentChip.transform.localScale.y,
                                                                      parentChip.transform.localScale.z + incrementStackBy);
                        ChipData newChipData = new ChipData(chipToMake.GetComponent<Chip>().chipData.ChipValue);
                        parentChip.GetComponent<Chip>().chipStack.chips.Add(newChipData);
                        parentChip.GetComponent<Chip>().chipStack.stackValue += newChipData.ChipValue;
                    }
                }
            }
        }
        Vector3 trueOffset = firstStackPos - lastStackPos;
        chipContainer.transform.position += trueOffset / 2;
    }

    public void ConsolidateStack(List<Chip> chipsToConsolidate)
    {
        if (chipsToConsolidate.Count > 1)
        {
            for (int i = 0; i < chipsToConsolidate.Count; i++)
            {
                Chip chip = chipsToConsolidate[i];
                if (chip.chipStack != null && chip.chipStack.chips.Count < 10)
                {
                    for (int chipToCheck = 0; chipToCheck < chipsToConsolidate.Count; chipToCheck++)
                    {
                        if (chipToCheck != i)
                        {
                            if (chipsToConsolidate[chipToCheck].chipStack != null && chipsToConsolidate[chipToCheck].chipData.ChipValue == chip.chipData.ChipValue)
                            {
                                for (int chipsToAdd = 0; chipsToAdd < chip.chipStack.chips.Count; chipsToAdd++)
                                {
                                    chipsToConsolidate[chipToCheck].chipStack.AddToStackInHand(chip.chipStack.chips[chipsToAdd]);
                                }
                                chipsToConsolidate.Remove(chip);
                                Debug.Log("BUG HERE SOMETIMES");
                                //trying to destroy chips that already are destoryed, null reference
                                if (chip != null)
                                {
                                    //Destroy(chip.gameObject);
                                    Services.ChipManager.chipsToDestroy.Add(chip.gameObject);
                                }
                                break;
                            }
                        }
                    }
                }
                else if (chip.chipStack == null)
                {
                    for (int chipToCheck = 0; chipToCheck < chipsToConsolidate.Count; chipToCheck++)
                    {
                        if (chipToCheck != i)
                        {
                            if (chipsToConsolidate[chipToCheck].chipStack != null)
                            {
                                if (chipsToConsolidate[chipToCheck].chipData.ChipValue == chip.chipData.ChipValue && chipsToConsolidate[chipToCheck].chipStack.chips.Count < 10)
                                {
                                    chipsToConsolidate[chipToCheck].chipStack.AddToStackInHand(chip.chipData);
                                    chipsToConsolidate.Remove(chip);
                                    //Destroy(chip.gameObject);
                                    Services.ChipManager.chipsToDestroy.Add(chip.gameObject);
                                    break;
                                }
                            }
                            else
                            {
                                chipsToConsolidate[chipToCheck].chipStack = new ChipStack(chipsToConsolidate[chipToCheck]);
                            }
                        }
                    }
                }
            }
            for (int i = 0; i < chipsToConsolidate.Count; i++)
            {
                chipsToConsolidate[i].spotIndex = i;
            }
        }
    }

    public List<Vector3> CreateChipPositionsForPushing(Vector3 startPosition, float xIncremenet, float zIncrement, int maxRowSize, int maxColumnSize)
    {
        List<Vector3> listOfPositions = new List<Vector3>();
        float xOffset;
        float zOffset;
        for (int i = 0; i < maxColumnSize; i++)
        {
            if (i % 2 == 0)
            {
                xOffset = (((i % maxRowSize) / 2) + 0.5f) * -xIncremenet;
            }
            else xOffset = (((i % maxRowSize) / 2) + 0.5f) * xIncremenet;

            zOffset = (i / maxRowSize) * zIncrement;

            listOfPositions.Add(new Vector3(startPosition.x + xOffset, 0, startPosition.z + zOffset));
        }

        return listOfPositions;
    }

    public List<Vector3> CreateChipPositionsForPot(Vector3 startPosition, float xIncrement, float zIncrement, int maxRowSize, int maxColumnSize, float yPos)
    {
        List<Vector3> listOfPositions = new List<Vector3>();
        float xOffset;
        float zOffset;
        for (int i = 0; i < maxColumnSize; i++)
        {
            if (i % 2 == 0)
            {
                zOffset = (((i % maxRowSize) / 2) + 0.5f) * -zIncrement;
            }
            else zOffset = (((i % maxRowSize) / 2) + 0.5f) * zIncrement;

            xOffset = (i / maxRowSize) * xIncrement;

            listOfPositions.Add(new Vector3(startPosition.x + xOffset, yPos, startPosition.z + zOffset));
        }
        return listOfPositions;
    }

    //this is just me ease-of-life function for findining the correct prefab
    public GameObject FindChipPrefab(int chipValue)
    {
        GameObject chipPrefab = null;
        switch (chipValue)
        {
            case ChipConfig.RED_CHIP_VALUE:
                chipPrefab = Services.PrefabDB.RedChip;
                break;
            case ChipConfig.BLUE_CHIP_VALUE:
                chipPrefab = Services.PrefabDB.BlueChip;
                break;
            case ChipConfig.WHITE_CHIP_VALUE:
                chipPrefab = Services.PrefabDB.WhiteChip;
                break;
            case ChipConfig.BLACK_CHIP_VALUE:
                chipPrefab = Services.PrefabDB.BlackChip;
                break;
            default:
                break;
        }
        return chipPrefab;
    }

    //this basically goes through a given chipValue and finds each instance of that chipValue in the playerChipStack
    public int FindChipMax(int chipValue, PokerPlayerRedux player)
    {
        int chipMax = 0;
        //Debug.Log("chipCount = " + chipCount);
        switch (chipValue)
        {
            case ChipConfig.BLACK_CHIP_VALUE:
                chipMax = (int)((float)(player.chipCount * 0.45f) / (float)ChipConfig.BLACK_CHIP_VALUE);
                break;
            case ChipConfig.WHITE_CHIP_VALUE:
                chipMax = (int)((float)(player.chipCount * 0.35f) / (float)ChipConfig.WHITE_CHIP_VALUE);
                break;
            case ChipConfig.BLUE_CHIP_VALUE:
                chipMax = (int)((float)(player.chipCount) * 0.15f / (float)ChipConfig.BLUE_CHIP_VALUE);
                break;
            case ChipConfig.RED_CHIP_VALUE:
                chipMax = (int)((float)(player.chipCount) / (float)ChipConfig.RED_CHIP_VALUE); //took out multiplying the chipCount by .05f
                break;
            default:
                break;
        }
        return chipMax;
    }
}
