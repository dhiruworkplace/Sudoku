using UnityEngine;
using UnityEngine.UI;

namespace ArtboxGames
{
    public class SelectPlayer : MonoBehaviour
    {
        [SerializeField] private Toggle twoPlayer;
        [SerializeField] private Toggle threePlayer;
        [SerializeField] private Toggle fourPlayer;

        private void OnEnable()
        {
            if (GameManager.Instance.requiredPlayer == 2)
            {
                twoPlayer.isOn = true;
                threePlayer.isOn = false;
                fourPlayer.isOn = false;
            }
            else if (GameManager.Instance.requiredPlayer == 3)
            {
                twoPlayer.isOn = false;
                threePlayer.isOn = true;
                fourPlayer.isOn = false;
            }
            else if (GameManager.Instance.requiredPlayer == 4)
            {
                twoPlayer.isOn = false;
                threePlayer.isOn = false;
                fourPlayer.isOn = true;
            }
        }

        private void OnDisable()
        {
            twoPlayer.isOn = false;
            threePlayer.isOn = false;
            fourPlayer.isOn = false;
        }
    }
}