using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class RandomMaster : MonoBehaviour
    {
        public int randomCount = 100;
        public List<float> chances = new List<float>();
        public AnimationCurve generationCurve;
        public int maxLevel = 3;

        public bool check = false;

        void OnValidate()
        {
            if (check)
            {
                check = false;

                HandleRandom();
            }
        }

        void HandleRandom()
        {
            int item0 = 0;
            int item1 = 0;
            int item2 = 0;
            int item3 = 0;
            int item4 = 0;
            int item5 = 0;
            int item6 = 0;
            int item7 = 0;
            int item8 = 0;
            int item9 = 0;

            for (int i = 0; i < randomCount; i++)
            {
                int selectedItem;

                float randomFloat = Glob.CalcCurvedChances(generationCurve) * maxLevel;

                selectedItem = Mathf.FloorToInt(randomFloat);

                switch (selectedItem)
                {
                    case 1:
                        item1++;
                        break;
                    case 2:
                        item2++;
                        break;
                    case 3:
                        item3++;
                        break;
                    case 4:
                        item4++;
                        break;
                    case 5:
                        item5++;
                        break;
                    case 6:
                        item6++;
                        break;
                    case 7:
                        item7++;
                        break;
                    case 8:
                        item8++;
                        break;
                    case 9:
                        item9++;
                        break;
                    default:
                        item0++;
                        break;
                }
            }

            Debug.Log("0: " + item0);
            Debug.Log("1: " + item1);
            Debug.Log("2: " + item2);
            Debug.Log("3: " + item3);
            Debug.Log("4: " + item4);
            Debug.Log("5: " + item5);
            Debug.Log("6: " + item6);
            Debug.Log("7: " + item7);
            Debug.Log("8: " + item8);
            Debug.Log("9: " + item9);


            /* int a = 0;
             int b = 0;
             int c = 0;

            for (int i = 0; i < randomCount; i++)
            {
                System.Random random = new();
                double diceRoll = random.NextDouble();
                double cumulative = 0.0;

                for (int j = 0; j < chances.Count; j++)
                {
                    cumulative += chances[j] / 100;

                    if (diceRoll < cumulative)
                    {
                        if (j == 0)
                        {
                            a++;
                        }
                        else if (j == 1)
                        {
                            b++;
                        }
                        else
                        {
                            c++;
                        }

                        break;
                    }
                }
            }

           Debug.Log("A: " + a);
             Debug.Log("B: " + b);
             Debug.Log("C: " + c);*/
        }
    }
}
