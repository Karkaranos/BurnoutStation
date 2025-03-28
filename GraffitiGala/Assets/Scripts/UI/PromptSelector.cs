using TMPro;
using UnityEngine;
public class PromptSelector : MonoBehaviour
{
    public TMP_Text promptText;
    string[] promptList = {"Neighborhood", "Alien", "Nature", "Friends", "City", "Surprise", "Rich", "Happiness", "Dancing", "Cozy", "Annoying", "Ancient", "Future",
    "Fruits", "Farm", "Home", "Dream", "Smart", "Insects", "Flowers", "Clothing", "Investigation", "Pirates", "Thanksgiving", "Spooky", "Myths", "Chaos",
    "Jungle", "Circles", "Elements", "Funny", "Electricity", "Fuzzy", "Monster", "Silly", "Whimsy", "Favourite, 'Random", "Wonder", "School", "Fast Food", "Ocean", "Small"};


    // Start is called before the first frame update
    void Start()
    {
        randomPrompt();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void randomPrompt()
    {
        int p = Random.Range(1, promptList.Length);
        string prompt = "Your prompt: " + promptList[p];
        promptText.SetText(prompt);
        print(promptList[p]);
    }

}
