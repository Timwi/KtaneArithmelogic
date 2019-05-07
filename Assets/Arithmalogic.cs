using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using KModkit;


public class Arithmalogic : MonoBehaviour
{
    public KMBombInfo Bomb;
    public KMAudio Audio;
    public KMBombModule Module;

    private static int _moduleIdCounter = 1;
    private int _moduleId;

    public KMSelectable submit;
    public KMSelectable buttonA;
    public KMSelectable buttonB;
    public KMSelectable buttonC;

    //public MeshRenderer display;
        
    //public KMSelectable button;

    public MeshRenderer meshSymbolA;
    public MeshRenderer meshSymbolB;
    public MeshRenderer meshSymbolC;
    public MeshRenderer meshSymbolSubmit;

    public MeshRenderer meshNumberA;
    public MeshRenderer meshNumberB;
    public MeshRenderer meshNumberC;

    public MeshRenderer abOpen;
    public MeshRenderer abOper;
    public MeshRenderer abClosed;
    public MeshRenderer bcOpen;
    public MeshRenderer bcOper;
    public MeshRenderer bcClosed;

    //public KMRuleSeedable RuleSeedable;
    string[] symbols = new string[22]
    { "©", "Ѯ", "★", "Җ", "Ѭ", "₠", "Ϡ", "Ѧ",
      "æ", "Ԇ", "ϫ", "Ӭ", "Ͼ",  "Ѫ", "Ҩ",
      "Ϙ", "ζ", "Ͽ", "ƛ", "€", "☆", "œ"
    };
    //"the symbol is equal to " + 
    string[] symbolValueNames = new string[22]
    {
        "the submit symbol's position in the manual's table",
        "the earliest position of serial number letters in the English alphabet",
        "the average serial number digit, rounded up",
        "the number of indicators",
        "the number of battery holders",
        "triple the number of lit indicators",
        "the day of the month when the bomb was activated",
        "the lowest serial number digit plus five",
        "the total number of ports",
        "the number of unlit indicators times four",
        "the number of serial number consonants times five",
        "the number of batteries",
        "the sum of the serial number's digits",
        "the latest position of serial number letters in the English alphabet",
        "the total number of modules on bomb modulo 25",
        "the number of serial number vowels times six",
        "the number of batteries plus indicators",
        "the number of lit indicators plus port plates",
        "the number of port plates",
        "the highest serial number digit",
        "fifteen",
        "the number of unlit indicators plus battery holders"
    };

    //"a number is true if " + 

    string[] symbolConditionNames = new string[22]
    {
        "that number is even",
        "that number is a multiple of 7 or 13",
        "that number modulo 3 = 1",
        "that number is prime",
        "any of that number's digits are odd",
        "that number modulo 5 = 2 or 4",
        "that number's digits add up to 9 to 13",
        "that number's last two digits are within two of each other",
        "that number modulo 7 = 1, 3, or 6",
        "that number contains a 3 or 6",
        "that number's digital root is odd",
        "that number is a multiple of 4",
        "that number's digits add up to an odd number",
        "that number is odd",
        "that number's digits don't add up to 7 to 11",
        "that number is a multiple of 6",
        "that number's digital root is even",
        "that number is composite",
        "that number's digits add up to an even number",
        "that number contains a 2 or 9",
        "that number modulo 4 = 1",
        "that number's last two digits are at least five apart"
    };

    //sums can't be lower than 10 so there
    int[] primes = new int[]
    {
       11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47,
        53, 59, 61, 67, 71, 73, 79, 83, 89, 97, 101,
        103, 107, 109, 113, 127, 131, 137, 139, 149,
        151, 157, 163, 167, 173, 179, 181, 191, 193, 197, 199
    };
    // please don't have a thousand widgets on your bombs

    public bool OP_AND(bool a, bool b) { return a && b; }
    public bool OP_NAND(bool a, bool b) { return !OP_AND(a, b); }
    public bool OP_OR(bool a, bool b) { return a || b; }
    public bool OP_NOR(bool a, bool b) { return !OP_OR(a, b); }
    public bool OP_XOR(bool a, bool b) { return a ^ b; }
    public bool OP_XNOR(bool a, bool b) { return !OP_XOR(a, b); }
    public bool OP_LIMP(bool a, bool b) { return !(a && !b); }
    public bool OP_RIMP(bool a, bool b) { return !(!a && b); }
	
	string[] operators = new string[] { " ∧ ", " ∨ ", " ⊻ ",   "→",   " | ", " ↓ ", "↔",    "←" };
                                    //   and     or     xor   implies   nand   nor   xnor  implied by

    string[] alphabet = new string[26]
    { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M",
      "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z"};

    /*
    int[] values = new int[26] 
    { 1, 5, 5, 5, 1, 5, 5, 6, 1, 3, 6, 4, 6,
      4, 1, 6, 3, 4, 4, 4, 1, 6, 2, 3, 2, 3 };
  
         */


    bool pressedAllowed = false;

    // TWITCH PLAYS SUPPORT
    //int tpStages; This one is not needed for this module
    // TWITCH PLAYS SUPPORT

    int[] aValues = new int[4];
	int[] bValues = new int[4];
	int[] cValues = new int[4];

	int aSymbol;
	int bSymbol;
	int cSymbol;
	int submitSymbol;

    int aValue;
    int bValue;
    int cValue;

    int abOperator;
    int bcOperator;
    bool abParen;

    int currentDisplayA;
    int currentDisplayB;
    int currentDisplayC;

    bool finalA;
    bool finalB;
    bool finalC;

    bool isSolved = false;
    bool tpActive = false;
    
    void Start()
    {
        _moduleId = _moduleIdCounter++;
        //colorblindModeEnabled = colorblindMode.ColorblindModeActive;
        Init();
        pressedAllowed = true;
    }

    void Init()
    {
        delegationZone();
            //0       1      2       3       4      5      6       7  
        //   and     or     xor   implies   nand   nor   xnor  implied by
        abOperator = UnityEngine.Random.Range(0, 8);
        bcOperator = UnityEngine.Random.Range(0, 8);
        abOper.GetComponentInChildren<TextMesh>().text = operators[abOperator];
        bcOper.GetComponentInChildren<TextMesh>().text = operators[bcOperator];
        if (UnityEngine.Random.Range(0, 2) == 0)
        {
            abParen = true;
            abOpen.GetComponentInChildren<TextMesh>().text = "(";
            abClosed.GetComponentInChildren<TextMesh>().text = ")";
            bcOpen.GetComponentInChildren<TextMesh>().text = "";
            bcClosed.GetComponentInChildren<TextMesh>().text = "";
        }
        else
        {
            abParen = false;
            abOpen.GetComponentInChildren<TextMesh>().text = "";
            abClosed.GetComponentInChildren<TextMesh>().text = "";
            bcOpen.GetComponentInChildren<TextMesh>().text = "(";
            bcClosed.GetComponentInChildren<TextMesh>().text = ")";
        }
        aSymbol = UnityEngine.Random.Range(0, symbols.Count());
        bSymbol = aSymbol;
        cSymbol = aSymbol;
        submitSymbol = aSymbol;

        meshSymbolA.GetComponentInChildren<TextMesh>().text = symbols[aSymbol];

        while (bSymbol == aSymbol)
        {
            bSymbol = UnityEngine.Random.Range(0, symbols.Count());
        }

        while (cSymbol == aSymbol || cSymbol == bSymbol)
        {
            cSymbol = UnityEngine.Random.Range(0, symbols.Count());
        }

        while (submitSymbol == aSymbol || submitSymbol == bSymbol || submitSymbol == cSymbol)
        {
            submitSymbol = UnityEngine.Random.Range(0, symbols.Count());
        }
        //submitSymbol = 21; // debug

        aValue = giveSymbolValue(aSymbol);
        bValue = giveSymbolValue(bSymbol);
        cValue = giveSymbolValue(cSymbol);

        aValues[0] = UnityEngine.Random.Range(10, 22);
        bValues[0] = UnityEngine.Random.Range(10, 22);
        cValues[0] = UnityEngine.Random.Range(10, 22);

        for (int vSet = 1; vSet < 4; vSet++)
        {
            aValues[vSet] = aValues[vSet - 1] + UnityEngine.Random.Range(4, 10);
            bValues[vSet] = bValues[vSet - 1] + UnityEngine.Random.Range(4, 10);
            cValues[vSet] = cValues[vSet - 1] + UnityEngine.Random.Range(4, 10);
        }

        //make sure there's at least one true total and at least one false total for each display
        while (
            (determineNumberTruth(aValues[0] + aValue) && determineNumberTruth(aValues[1] + aValue) && determineNumberTruth(aValues[2] + aValue) && determineNumberTruth(aValues[3] + aValue)) ||
            (!determineNumberTruth(aValues[0] + aValue) && !determineNumberTruth(aValues[1] + aValue) && !determineNumberTruth(aValues[2] + aValue) && !determineNumberTruth(aValues[3] + aValue))
            )
        {
            aValues[0] = UnityEngine.Random.Range(10, 22);
            for (int vSet = 1; vSet < 4; vSet++)
            {
                aValues[vSet] = aValues[vSet - 1] + UnityEngine.Random.Range(4, 10);
            }
        }
        while (
            (determineNumberTruth(bValues[0] + bValue) && determineNumberTruth(bValues[1] + bValue) && determineNumberTruth(bValues[2] + bValue) && determineNumberTruth(bValues[3] + bValue)) ||
            (!determineNumberTruth(bValues[0] + bValue) && !determineNumberTruth(bValues[1] + bValue) && !determineNumberTruth(bValues[2] + bValue) && !determineNumberTruth(bValues[3] + bValue))
            )
        {
            bValues[0] = UnityEngine.Random.Range(10, 22);
            for (int vSet = 1; vSet < 4; vSet++)
            {
                bValues[vSet] = bValues[vSet - 1] + UnityEngine.Random.Range(4, 10);
            }
        }
        while (
            (determineNumberTruth(cValues[0] + cValue) && determineNumberTruth(cValues[1] + cValue) && determineNumberTruth(cValues[2] + cValue) && determineNumberTruth(cValues[3] + cValue)) ||
            (!determineNumberTruth(cValues[0] + cValue) && !determineNumberTruth(cValues[1] + cValue) && !determineNumberTruth(cValues[2] + cValue) && !determineNumberTruth(cValues[3] + cValue))
            )
        {
            cValues[0] = UnityEngine.Random.Range(10, 22);
            for (int vSet = 1; vSet < 4; vSet++)
            {
                cValues[vSet] = cValues[vSet - 1] + UnityEngine.Random.Range(4, 10);
            }
        }

        meshSymbolB.GetComponentInChildren<TextMesh>().text = symbols[bSymbol];
        meshSymbolC.GetComponentInChildren<TextMesh>().text = symbols[cSymbol];
        meshSymbolSubmit.GetComponentInChildren<TextMesh>().text = symbols[submitSymbol];

        currentDisplayA = 0;
        currentDisplayB = 0;
        currentDisplayC = 0;

        meshNumberA.GetComponentInChildren<TextMesh>().text = aValues[0].ToString();
        meshNumberB.GetComponentInChildren<TextMesh>().text = bValues[0].ToString();
        meshNumberC.GetComponentInChildren<TextMesh>().text = cValues[0].ToString();

        Debug.LogFormat("[Arithmelogic #{0}] The given statement is {1} {2} {3} {4} {5} {6} {7} {8} {9}.", _moduleId, 
            abParen ? "(" : "", 
            symbols[aSymbol], 
            operators[abOperator],
            abParen ? "" : "(",
            symbols[bSymbol],
            abParen ? ")" : "",
            operators[bcOperator],
            symbols[cSymbol],
            abParen ? "" : ")"
            );
        Debug.LogFormat("[Arithmelogic #{0}] (Table positions are {1}, {2}, and {3} for A, B, and C respectively.)", _moduleId, aSymbol + 1, bSymbol + 1, cSymbol + 1);
        Debug.LogFormat("[Arithmelogic #{0}] A's symbol, {1}, is equal to {2}, which is {3}.", _moduleId, symbols[aSymbol], symbolValueNames[aSymbol], aValue);
        Debug.LogFormat("[Arithmelogic #{0}] B's symbol, {1}, is equal to {2}, which is {3}.", _moduleId, symbols[bSymbol], symbolValueNames[bSymbol], bValue);
        Debug.LogFormat("[Arithmelogic #{0}] C's symbol, {1}, is equal to {2}, which is {3}.", _moduleId, symbols[cSymbol], symbolValueNames[cSymbol], cValue);

        Debug.LogFormat("[Arithmelogic #{0}] The submit button's symbol, {1}, means a number is true if {2}.", _moduleId, symbols[submitSymbol], symbolConditionNames[submitSymbol]);

        Debug.LogFormat("[Arithmelogic #{0}] A's values are {1} (becomes {2} which is {3}), {4} (becomes {5} which is {6}), {7} (becomes {8} which is {9}), and {10} (becomes {11} which is {12})."
            , _moduleId, aValues[0], (aValues[0] + aValue), determineNumberTruth(aValues[0] + aValue),
            aValues[1], (aValues[1] + aValue), determineNumberTruth(aValues[1] + aValue),
            aValues[2], (aValues[2] + aValue), determineNumberTruth(aValues[2] + aValue),
            aValues[3], (aValues[3] + aValue), determineNumberTruth(aValues[3] + aValue));
        Debug.LogFormat("[Arithmelogic #{0}] B's values are {1} (becomes {2} which is {3}), {4} (becomes {5} which is {6}), {7} (becomes {8} which is {9}), and {10} (becomes {11} which is {12})."
            , _moduleId, bValues[0], (bValues[0] + bValue), determineNumberTruth(bValues[0] + bValue),
            bValues[1], (bValues[1] + bValue), determineNumberTruth(bValues[1] + bValue),
            bValues[2], (bValues[2] + bValue), determineNumberTruth(bValues[2] + bValue),
            bValues[3], (bValues[3] + bValue), determineNumberTruth(bValues[3] + bValue));
        Debug.LogFormat("[Arithmelogic #{0}] C's values are {1} (becomes {2} which is {3}), {4} (becomes {5} which is {6}), {7} (becomes {8} which is {9}), and {10} (becomes {11} which is {12})."
            , _moduleId, cValues[0], (cValues[0] + cValue), determineNumberTruth(cValues[0] + cValue),
            cValues[1], (cValues[1] + cValue), determineNumberTruth(cValues[1] + cValue),
            cValues[2], (cValues[2] + cValue), determineNumberTruth(cValues[2] + cValue),
            cValues[3], (cValues[3] + cValue), determineNumberTruth(cValues[3] + cValue));


        pressedAllowed = true;
    }


    void OnHold()
    {
		
    }

    void OnRelease()
    {

    }
    /*
        private void FixedUpdate()
        {
            if (isSolved)
            {

            }
        }
    */


#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Use !{0} (submit/s) 97 98 99 to submit 97 for the first display's shown number, 98 for the second, and 99 for the third. Use !{0} cycle A/B/C to cycle that display, and !{0} cycle all to cycle all displays.";
    private readonly bool TwitchShouldCancelCommand = false;
#pragma warning restore 414

    private IEnumerator ProcessTwitchCommand(string command)
    {

        tpActive = true;
        var pieces = command.ToLowerInvariant().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        


        string theError;
        theError = "";
        if (pieces.Count() == 0)
        {
            theError = "sendtochaterror No arguments! You need to use submit/s 97 98 99 to submit or cycle A/B/C/all to cycle.";
            yield return theError;
        }
        else if (pieces.Count() < 2)
        {
            theError = "sendtochaterror Not enough arguments! You need to use submit/s 97 98 99 to submit or cycle A/B/C/all to cycle";
            yield return theError;
        }
        else if (pieces[0] != "submit" && pieces[0] != "s" && pieces[0] != "cycle" && pieces[0] != "c")
        {
            theError = "sendtochaterror Invalid argument! You need to use submit/s 97 98 99 to submit or cycle A/B/C/all to cycle";
            yield return theError;
        }
        else if (pieces.Length > 1 && (pieces[0] == "cycle" || pieces[0] == "c"))
        {
            if (pieces[1] == "all")
            {
                yield return null;
                for (int bNum = 0; bNum < 3; bNum++)
                {
                    for (int cNum = 0; cNum < 4; cNum++)
                    {

                            yield return new WaitForSeconds(1f);
                            yield return "trycancel";
                            switch (bNum)
                            {
                                case 0:
                                    buttonA.OnInteract();
                                    break;
                                case 1:
                                    buttonB.OnInteract();
                                    break;
                                default:
                                    buttonC.OnInteract();
                                    break;
                            }
                        if (TwitchShouldCancelCommand)
                        {
                            cNum = 4;
                            bNum = 3;
                            yield return "cancelled";
                        }
                    }
                }

            }
            else if (pieces[1] == "a")
            {
                yield return null;
                for (int cNum = 0; cNum < 4; cNum++)
                {

                        yield return new WaitForSeconds(1f);
                        yield return "trycancel";
                        buttonA.OnInteract();

                    if (TwitchShouldCancelCommand)
                    {
                        cNum = 4;
                        yield return "cancelled";
                    }
                }
            }
            else if (pieces[1] == "b")
            {
                yield return null;
                for (int cNum = 0; cNum < 4; cNum++)
                {
                        yield return new WaitForSeconds(1f);
                        yield return "trycancel";
                        buttonB.OnInteract();

                    if (TwitchShouldCancelCommand)
                    {
                        cNum = 4;
                        yield return "cancelled";
                    }
                }
            }
            else if (pieces[1] == "c")
            {
                yield return null;
                for (int cNum = 0; cNum < 4; cNum++)
                {
                        yield return new WaitForSeconds(1f);
                        yield return "trycancel";
                        buttonC.OnInteract();

                    if (TwitchShouldCancelCommand)
                    {
                        cNum = 4;
                        yield return "cancelled";
                    }
                }
            }
            else
            {
                theError = "sendtochaterror Invalid argument! You need to use cycle (a, b, c, or all) to cycle";
                yield return theError;
            }
        }
        else if (pieces.Length > 1 && (pieces[0] == "submit" || pieces[0] == "s"))
        {
            var failedSubmit = true;
            for (int cNum = 0; cNum < 4; cNum++)
            {
                if (aValues[cNum] == pieces[1].TryParseInt())
                {
                    failedSubmit = false;
                }
            }
            if (failedSubmit)
            {
                theError = "sendtochaterror Invalid argument for position A! " + pieces[1] + " was not a valid displayed number.";
                yield return theError;
            }
            failedSubmit = true;
            for (int cNum = 0; cNum < 4; cNum++)
            {
                if (bValues[cNum] == pieces[2].TryParseInt())
                {
                    failedSubmit = false;
                }
            }
            if (failedSubmit)
            {
                theError = "sendtochaterror Invalid argument for position B! " + pieces[2] + " was not a valid displayed number.";
                yield return theError;
            }
            failedSubmit = true;
            for (int cNum = 0; cNum < 4; cNum++)
            {
                if (cValues[cNum] == pieces[3].TryParseInt())
                {
                    failedSubmit = false;
                }
            }
            if (failedSubmit)
            {
                theError = "sendtochaterror Invalid argument for position C! " + pieces[3] + " was not a valid displayed number.";
                yield return theError;
            }
            while (aValues[currentDisplayA] != pieces[1].TryParseInt())
            {
                yield return new WaitForSeconds(.1f);
                yield return null;
                buttonA.OnInteract();
            }
            while (bValues[currentDisplayB] != pieces[2].TryParseInt())
            {
                yield return new WaitForSeconds(.1f);
                yield return null;
                buttonB.OnInteract();
            }
            while (cValues[currentDisplayC] != pieces[3].TryParseInt())
            {
                yield return new WaitForSeconds(.1f);
                yield return null;
                buttonC.OnInteract();
            }
            yield return new WaitForSeconds(.1f);
            yield return null;
            submit.OnInteract();
        }
    }


    void doPressA()
    {
        currentDisplayA++;
        currentDisplayA = currentDisplayA % 4;
        meshNumberA.GetComponentInChildren<TextMesh>().text = aValues[currentDisplayA].ToString();
    }

    void doPressB()
    {

        currentDisplayB++;
        currentDisplayB = currentDisplayB % 4;
        meshNumberB.GetComponentInChildren<TextMesh>().text = bValues[currentDisplayB].ToString();
    }

    void doPressC()
    {
        currentDisplayC++;
        currentDisplayC = currentDisplayC++ % 4;
        meshNumberC.GetComponentInChildren<TextMesh>().text = cValues[currentDisplayC].ToString();
    }

    void doSubmit()
    {
        if (pressedAllowed)
        {
            var finalAValue = aValue + aValues[currentDisplayA];
            var finalBValue = bValue + bValues[currentDisplayB];
            var finalCValue = cValue + cValues[currentDisplayC];
            finalA = determineNumberTruth(finalAValue);
            finalB = determineNumberTruth(finalBValue);
            finalC = determineNumberTruth(finalCValue);
            //aValue = aSymbol + aValues[currentDisplayA];
            //bValue = bSymbol + bValues[currentDisplayB];
            //cValue = cSymbol + cValues[currentDisplayC];
            Debug.LogFormat("[Arithmelogic #{0}] A's symbol value, {1}, plus the displayed number for A, {2}, is equal to {3}, which is {4}.", 
                _moduleId, aValue, aValues[currentDisplayA], finalAValue, finalA.ToString());
            Debug.LogFormat("[Arithmelogic #{0}] B's symbol value, {1}, plus the displayed number for B, {2}, is equal to {3}, which is {4}.", 
                _moduleId, bValue, bValues[currentDisplayB], finalBValue, finalB.ToString());
            Debug.LogFormat("[Arithmelogic #{0}] C's symbol value, {1}, plus the displayed number for C, {2}, is equal to {3}, which is {4}.", 
                _moduleId, cValue, cValues[currentDisplayC], finalCValue, finalC.ToString());
            var wrongString = "";
            finalA = determineNumberTruth(finalAValue);
            finalB = determineNumberTruth(finalBValue);
            finalC = determineNumberTruth(finalCValue);
            var interValue = false;
            var finalValue = false;
            if (abParen)
            {
                interValue = figureTruth(finalA, finalB, abOperator);
                finalValue = figureTruth(interValue, finalC, bcOperator);
                Debug.LogFormat("[Arithmelogic #{0}] A {2} B, or ({1} {2} {3}), is {4}, and {4} {5} {6} is {7}.",
                    _moduleId, finalA.ToString(), operators[abOperator], finalB.ToString(), interValue.ToString(),
                    operators[bcOperator], finalC.ToString(), finalValue.ToString());
            }
            else
            {
                interValue = figureTruth(finalB, finalC, bcOperator);
                finalValue = figureTruth(finalA, interValue, abOperator);
                Debug.LogFormat("[Arithmelogic #{0}] B {2} C, or ({1} {2} {3}), is {4}, and {5} {6} {4} is {7}.",
                    _moduleId, finalB.ToString(), operators[bcOperator], finalC.ToString(), interValue.ToString(),
                    finalA.ToString(), operators[abOperator], finalValue.ToString());
            }
            if (!finalValue)
            {
                wrongString = "The statement was false! ";
            }
            for (int testingA = 3; testingA > currentDisplayA; testingA--)
            {
                if (determineNumberTruth(aValue + aValues[testingA]) == finalA)
                {
                    wrongString = wrongString + "Element A's display could have a higher value (" + aValues[testingA] + " as opposed to the selected " + aValues[currentDisplayA] + " and A would still be " + finalA.ToString() + ". ";
                    testingA = currentDisplayA;
                }
            }
            for (int testingB = 3; testingB > currentDisplayB; testingB--)
            {
                if (determineNumberTruth(bValue + bValues[testingB]) == finalB)
                {
                    wrongString = wrongString + "Element B's display could have a higher value (" + bValues[testingB] + " as opposed to the selected " + bValues[currentDisplayB] + " and B would still be " + finalB.ToString() + ". ";
                    testingB = currentDisplayB;
                }
            }
            for (int testingC = 3; testingC > currentDisplayC; testingC--)
            {
                if (determineNumberTruth(cValue + cValues[testingC]) == finalC)
                {
                    wrongString = wrongString + "Element C's display could have a higher value (" + cValues[testingC] + " as opposed to the selected " + cValues[currentDisplayC] + " and C would still be " + finalC.ToString() + ". ";
                    testingC = currentDisplayC;
                }
            }
            if (wrongString == "")
            {
                Debug.LogFormat("[Arithmelogic #{0}] No higher displayed numbers for A, B, or C could be selected which would keep their respective truth values the same, and the statement is true, so the module is disarmed!", _moduleId);
                pressedAllowed = false;
                isSolved = true;
                Module.HandlePass();
            }
            else
            {
                Debug.LogFormat("[Arithmelogic #{0}] {1}Strike given!", _moduleId, wrongString);
                wrongString = "";
                Module.HandleStrike();
            }
        }
    }

    bool figureTruth(bool valA, bool valB, int ourOperator)
    {

                                         //     0       1        2       3       4      5      6       7
        //string[] operators = new string[] { " ∧ ", " ∨ ",   " ⊻ ",  "→",    " | ", " ↓ ",  "↔",   "←" };
                                           //   and     or      xor   implies   nand   nor   xnor  implied by
                                           //////////////////////////////////////////////////////////////////
                                           //                          Right                 Double  Left
                                           //                          arrow                 arrow   arrow
        switch (ourOperator)
        {
            case 0:
                return OP_AND(valA, valB);
            case 1:
                return OP_OR(valA, valB);
            case 2:
                return OP_XOR(valA, valB);
            case 3:
                return OP_LIMP(valA, valB);
            case 4:
                return OP_NAND(valA, valB);
            case 5:
                return OP_NOR(valA, valB);
            case 6:
                return OP_XNOR(valA, valB);
            case 7:
                return OP_RIMP(valA, valB);
            default:
                return true;
        }

    }

    bool determineNumberTruth(int theNumber)
    {
        var returnedBool = false;
        switch (submitSymbol)
        {
            case 0: //even
                if (theNumber % 2 == 0)
                {
                    returnedBool = true;
                }
                return returnedBool;
            case 1: //multiple of 7 or 13
                if (theNumber % 7 == 0 || theNumber % 13 == 0)
                {
                    returnedBool = true;
                }
                return returnedBool;
            case 2: //N modulo 3 is 1
                if (theNumber % 3 == 1)
                {
                    returnedBool = true;
                }
                return returnedBool;
            case 3: //prime
                for (int primeCheck = 0; primeCheck < primes.Length; primeCheck++)
                {
                    if (theNumber == primes[primeCheck])
                    {
                        returnedBool = true;
                        primeCheck = primes.Length;
                    }

                }
                return returnedBool;
            case 4: //any digit is odd
                for (int oddCheck = 0; oddCheck < theNumber.ToString().Length; oddCheck++)
                {
                    if (theNumber.ToString().Substring(oddCheck, 1) == "1" || theNumber.ToString().Substring(oddCheck, 1) == "3" || theNumber.ToString().Substring(oddCheck, 1) == "5" ||
                        theNumber.ToString().Substring(oddCheck, 1) == "7" || theNumber.ToString().Substring(oddCheck, 1) == "9")
                    {
                        returnedBool = true;
                        oddCheck = theNumber.ToString().Length;
                    }
                }
                return returnedBool;
            case 5: //N modulo 5 = 2 or 4
                if (theNumber % 5 == 2 || theNumber % 5 == 4)
                {
                    returnedBool = true;
                }
                return returnedBool;
            case 6: //numbers add to 9 to 13
                var finalNTNumber = 0;
                for (int NTThing = 0; NTThing < theNumber.ToString().Length; NTThing++)
                {
                    finalNTNumber = finalNTNumber + Int16.Parse(theNumber.ToString().Substring(NTThing, 1));
                }
                if (finalNTNumber >= 9 && finalNTNumber <= 13)
                {
                    returnedBool = true;
                }
                return returnedBool;
            case 7: //last digits within two
                if (theNumber.ToString().Substring(theNumber.ToString().Length - 1, 1).TryParseInt() - theNumber.ToString().Substring(theNumber.ToString().Length - 2, 1).TryParseInt() <= 2 &&
                    theNumber.ToString().Substring(theNumber.ToString().Length - 1, 1).TryParseInt() - theNumber.ToString().Substring(theNumber.ToString().Length - 2, 1).TryParseInt() >= -2)             
                {
                    returnedBool = true;
                }
                return returnedBool;
            case 8: //N modulo 7 = 1 3 or 6
                if (theNumber % 7 == 1 || theNumber % 7 == 3 || theNumber % 7 == 6)
                {
                    returnedBool = true;
                }
                return returnedBool;
            case 9: //contains 3 or 6
                for (int tsCheck = 0; tsCheck < theNumber.ToString().Length; tsCheck++)
                {
                    if (theNumber.ToString().Substring(tsCheck, 1) == "3" || theNumber.ToString().Substring(tsCheck, 1) == "6")
                    {
                        returnedBool = true;
                        tsCheck = theNumber.ToString().Length;
                    }
                }
                return returnedBool;
            case 10: //digital root is odd
                var finalDRNumber = 0;
                for (int drThing = 0; drThing < theNumber.ToString().Length; drThing++)
                {
                    finalDRNumber = finalDRNumber + Int16.Parse(theNumber.ToString().Substring(drThing, 1));
                }
                while (finalDRNumber > 9)
                {
                    var testNumber = finalDRNumber;
                    finalDRNumber = 0;
                    for (int drThing = 0; drThing < testNumber.ToString().Length; drThing++)
                    {
                        finalDRNumber = finalDRNumber + Int16.Parse(testNumber.ToString().Substring(drThing, 1));
                    }
                }
                if (finalDRNumber % 2 == 1)
                {
                    returnedBool = true;
                }
                return returnedBool;
            case 11: //mult of 4
                if (theNumber % 4 == 0)
                {
                    returnedBool = true;
                }
                return returnedBool;
            case 12: //digits add to odd
                var finalOTNumber = 0;
                for (int OTThing = 0; OTThing < theNumber.ToString().Length; OTThing++)
                {
                    finalOTNumber = finalOTNumber + Int16.Parse(theNumber.ToString().Substring(OTThing, 1));
                }
                if (finalOTNumber % 2 == 1)
                {
                    returnedBool = true;
                }
                return returnedBool;
            case 13: //odd
                if (theNumber % 2 != 0)
                {
                    returnedBool = true;
                }
                return returnedBool;
            case 14: //digits don't add to 7 to 11
                var finalSENumber = 0;
                for (int SEThing = 0; SEThing < theNumber.ToString().Length; SEThing++)
                {
                    finalSENumber = finalSENumber + Int16.Parse(theNumber.ToString().Substring(SEThing, 1));
                }
                if (finalSENumber >= 12 || finalSENumber <= 6)
                {
                    returnedBool = true;
                }
                return returnedBool;
            case 15: //multiple of 6
                if (theNumber % 6 == 0)
                {
                    returnedBool = true;
                }
                return returnedBool;
            case 16: //dr is even
                var lastDRNumber = 0;
                for (int drThing = 0; drThing < theNumber.ToString().Length; drThing++)
                {
                    lastDRNumber = lastDRNumber + Int16.Parse(theNumber.ToString().Substring(drThing, 1));
                }
                while (lastDRNumber > 9)
                {
                    var testNumber = lastDRNumber;
                    lastDRNumber = 0;
                    for (int drThing = 0; drThing < testNumber.ToString().Length; drThing++)
                    {
                        lastDRNumber = lastDRNumber + Int16.Parse(testNumber.ToString().Substring(drThing, 1));
                    }
                }
                if (lastDRNumber % 2 == 0)
                {
                    returnedBool = true;
                }
                return returnedBool;
            case 17: //composite
                returnedBool = true;
                for (int primeCheck = 0; primeCheck < primes.Length; primeCheck++)
                {
                    if (theNumber == primes[primeCheck])
                    {
                        returnedBool = false;
                        primeCheck = primes.Length;
                    }

                }
                return returnedBool;
            case 18: //digits add to even
                var finalETNumber = 0;
                for (int ETThing = 0; ETThing < theNumber.ToString().Length; ETThing++)
                {
                    finalETNumber = finalETNumber + Int16.Parse(theNumber.ToString().Substring(ETThing, 1));
                }
                if (finalETNumber % 2 == 0)
                {
                    returnedBool = true;
                }
                return returnedBool;
            case 19: //contains 2 or 9
                for (int tsCheck = 0; tsCheck < theNumber.ToString().Length; tsCheck++)
                {
                    if (theNumber.ToString().Substring(tsCheck, 1) == "2" || theNumber.ToString().Substring(tsCheck, 1) == "9")
                    {
                        returnedBool = true;
                        tsCheck = theNumber.ToString().Length;
                    }
                }
                return returnedBool;
            case 20: //N modulo 4 = 1
                if (theNumber % 4 == 1)
                {
                    returnedBool = true;
                }
                return returnedBool;
            case 21: //last two digits at least five apart
                if (theNumber.ToString().Substring(theNumber.ToString().Length - 1, 1).TryParseInt() - theNumber.ToString().Substring(theNumber.ToString().Length - 2, 1).TryParseInt() <= -5 ||
                    theNumber.ToString().Substring(theNumber.ToString().Length - 1, 1).TryParseInt() - theNumber.ToString().Substring(theNumber.ToString().Length - 2, 1).TryParseInt() >= 5)
                {
                    returnedBool = true;
                }
                return returnedBool;
            default:
                return returnedBool;
        }
    }

    int giveSymbolValue(int ourSymbol)
    {
        var someNumber = 0;
        switch (ourSymbol)
        {
            case 0: //submit symbol pos. in table
                return submitSymbol + 1;
            case 1: //earliest SN letter position
                for (int curPos = 0; curPos < 26; curPos++)
                {
                    for (int snPos = 0; snPos < 6; snPos++)
                    {
                        if (Bomb.GetSerialNumber().Substring(snPos, 1) == alphabet[curPos])
                        {
                            someNumber = curPos + 1;
                            snPos = 6;
                            curPos = 26;
                        }
                    }
                }
                return someNumber;
            case 2: //average SN digit, round up
                if (Bomb.GetSerialNumberNumbers().Sum() % Bomb.GetSerialNumberNumbers().Count() == 0)
                {
                    return Bomb.GetSerialNumberNumbers().Sum() / Bomb.GetSerialNumberNumbers().Count();
                }
                else
                {
                    return (Bomb.GetSerialNumberNumbers().Sum() / Bomb.GetSerialNumberNumbers().Count()) + 1;
                }
            case 3: //indicators
                return Bomb.GetIndicators().Count();
            case 4: //holders
                return Bomb.GetBatteryHolderCount();
            case 5: //triple on indicators
                return 3 * Bomb.GetOnIndicators().Count();
            case 6: //day of month
                return DateTime.Now.Date.Day;
            case 7: //low sn digit plus five
                for (int curDigit = 0; curDigit < 10; curDigit++)
                {
                    for (int snPos = 0; snPos < 6; snPos++)
                    {
                        if (Bomb.GetSerialNumber().Substring(snPos, 1).TryParseInt() == curDigit)
                        {
                            someNumber = curDigit;
                            snPos = 6;
                            curDigit = 10;
                        }
                    }
                }
                return someNumber + 5;
            case 8: //ports
                return Bomb.GetPorts().Count();
            case 9: //unlits * 4
                return 4 * Bomb.GetOffIndicators().Count();
            case 10: //consonants * 5
                for (int snPos = 0; snPos < 6; snPos++)
                {
                    if (!(Bomb.GetSerialNumber().Substring(snPos, 1).TryParseInt() > -1) &&
                        Bomb.GetSerialNumber().Substring(snPos, 1) != "A" && Bomb.GetSerialNumber().Substring(snPos, 1) != "E" &&
                        Bomb.GetSerialNumber().Substring(snPos, 1) != "I" && Bomb.GetSerialNumber().Substring(snPos, 1) != "O" &&
                        Bomb.GetSerialNumber().Substring(snPos, 1) != "U")
                    {
                        someNumber++;
                    }
                }
                return someNumber * 5;
            case 11: //batteries
                return Bomb.GetBatteryCount();
            case 12: //sum of SN digits
                for (int snPos = 0; snPos < 6; snPos++)
                {
                    if (Bomb.GetSerialNumber().Substring(snPos, 1).TryParseInt() > 0)
                    {
                        someNumber = someNumber + Int16.Parse(Bomb.GetSerialNumber().Substring(snPos, 1));
                    }
                }
                return someNumber;
            case 13: //latest position
                for (int curPos = 25; curPos > -1; curPos--)
                {
                    for (int snPos = 0; snPos < 6; snPos++)
                    {
                        if (Bomb.GetSerialNumber().Substring(snPos, 1) == alphabet[curPos])
                        {
                            someNumber = curPos + 1;
                            snPos = 6;
                            curPos = -1;
                        }
                    }
                }
                return someNumber;
            case 14: //bomb modules
                return Bomb.GetModuleNames().Count() % 25;
            case 15: // SN vowels * 6
                for (int snPos = 0; snPos < 6; snPos++)
                {
                    if (!(Bomb.GetSerialNumber().Substring(snPos, 1).TryParseInt() > -1) &&
                        !(Bomb.GetSerialNumber().Substring(snPos, 1) != "A" && Bomb.GetSerialNumber().Substring(snPos, 1) != "E" &&
                        Bomb.GetSerialNumber().Substring(snPos, 1) != "I" && Bomb.GetSerialNumber().Substring(snPos, 1) != "O" &&
                        Bomb.GetSerialNumber().Substring(snPos, 1) != "U"))
                    {
                        someNumber++;
                    }
                }
                return someNumber * 6;
            case 16: //bat + ind
                return Bomb.GetBatteryCount() + Bomb.GetIndicators().Count();
            case 17: //lit + plates
                return Bomb.GetOnIndicators().Count() + Bomb.GetPortPlateCount();
            case 18: //plates
                return Bomb.GetPortPlateCount();
            case 19: //hi digit
                for (int curDigit = 10; curDigit > 0; curDigit--)
                {
                    for (int snPos = 0; snPos < 6; snPos++)
                    {
                        if (Bomb.GetSerialNumber().Substring(snPos, 1).TryParseInt() == curDigit)
                        {
                            someNumber = curDigit;
                            snPos = 6;
                            curDigit = 0;
                        }
                    }
                }
                return someNumber;
            case 20: //15
                return 15;
            case 21: //unlit + holders
                return Bomb.GetOffIndicators().Count() + Bomb.GetBatteryHolderCount();
            default:
                return ourSymbol;
        }
    }

    void delegationZone()
    {
        buttonA.OnInteract += delegate () { OnHold(); doPressA(); return false; };
        buttonA.OnInteractEnded += delegate () { OnRelease(); };
        
        buttonB.OnInteract += delegate () { OnHold(); doPressB(); return false; };
        buttonB.OnInteractEnded += delegate () { OnRelease(); };

        buttonC.OnInteract += delegate () { OnHold(); doPressC(); return false; };
        buttonC.OnInteractEnded += delegate () { OnRelease(); };



        submit.OnInteract += delegate () { OnHold(); doSubmit(); return false; };
        submit.OnInteractEnded += delegate () { OnRelease(); };
    }

    

}
