using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;

namespace ArtboxGames
{
    public class LongClickBtn : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        private bool pointerDown;
        private float pointerDownTimer;

        public float requireHoldTime;

        public UnityEvent onLongClick;
        public System.Action<int> OnButtonClick { get; set; }

        [SerializeField]
        private Image fillImage;

        GameManager gameManager;

        void Start()
        {
            gameManager = GameManager.Instance;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            pointerDown = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (pointerDown)
            {
                if (OnButtonClick != null)
                {
                    OnButtonClick(GetComponent<NumberButton>().number);
                }
                gameManager.firstInputNum = 0;
            }
            Reset();
        }

        private void Update()
        {
            if (pointerDown && gameManager.NumberFirstInput)
            {
                pointerDownTimer += Time.deltaTime;
                if (pointerDownTimer >= requireHoldTime)
                {
                    Reset();
                    gameManager.firstInputNum = GetComponent<NumberButton>().number;
                }
                fillImage.fillAmount = pointerDownTimer / requireHoldTime;
            }
        }

        private void Reset()
        {
            pointerDown = false;
            pointerDownTimer = 0;
            fillImage.fillAmount = pointerDownTimer / requireHoldTime;
        }
    }
}