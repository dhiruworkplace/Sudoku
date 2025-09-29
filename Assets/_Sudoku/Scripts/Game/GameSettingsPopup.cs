using UnityEngine;

namespace ArtboxGames
{
	public class GameSettingsPopup : Popup
	{
		#region Inspector Variables

		[Space]

		[SerializeField] private ToggleSlider noteDetectionToggle	= null;
		[SerializeField] private ToggleSlider keepScreenOnToggle		= null;
		[SerializeField] private ToggleSlider numFirstInputToggle		= null;
		[SerializeField] private ToggleSlider mistakeLimitToggle = null;
		[SerializeField] private ToggleSlider soundToggle = null;
		[SerializeField] private ToggleSlider vibrateToggle = null;
		[SerializeField] private ToggleSlider highlightConnectedCellToggle = null;
		[SerializeField] private ToggleSlider highlightSameNumbersToggle = null;

		#endregion

		#region Unity Methods

		private void Start()
		{
			noteDetectionToggle.SetToggle(GameManager.Instance.NoteDetection, false);
			keepScreenOnToggle.SetToggle(GameManager.Instance.KeepScreenOn, false);
			numFirstInputToggle.SetToggle(GameManager.Instance.NumberFirstInput, false);
			mistakeLimitToggle.SetToggle(GameManager.Instance.MistakeLimit, false);
			soundToggle.SetToggle(GameManager.Instance.Sound, false);
			vibrateToggle.SetToggle(GameManager.Instance.Vibrate, false);
			highlightConnectedCellToggle.SetToggle(GameManager.Instance.HighlightConnectedCell, false);
			highlightSameNumbersToggle.SetToggle(GameManager.Instance.HighlightSameNumbers, false);

			noteDetectionToggle.OnValueChanged	+= OnNoteDetectionToggleChanged;
			keepScreenOnToggle.OnValueChanged		+= OnKeepScreenOnToggleChanged;
			numFirstInputToggle.OnValueChanged	+= OnNumFirstInputToggleChanged;
			mistakeLimitToggle.OnValueChanged += OnMistakeLimitToggleChanged;
			soundToggle.OnValueChanged += OnSoundToggleChanged;
			vibrateToggle.OnValueChanged += OnVibrateToggleChanged;
			highlightConnectedCellToggle.OnValueChanged += OnHighlightConnectedCellToggleChanged;
			highlightSameNumbersToggle.OnValueChanged += OnHighlightSameNumbersToggleChanged;
		}

		#endregion

		#region Private Methods

		private void OnNoteDetectionToggleChanged(bool isOn)
		{
			GameManager.Instance.SetGameSetting("1", isOn);
		}

		private void OnKeepScreenOnToggleChanged(bool isOn)
		{
			GameManager.Instance.SetGameSetting("2", isOn);
		}

		private void OnNumFirstInputToggleChanged(bool isOn)
		{
			GameManager.Instance.SetGameSetting("3", isOn);
		}

		private void OnMistakeLimitToggleChanged(bool isOn)
		{
			GameManager.Instance.SetGameSetting("4", isOn);
		}

		private void OnSoundToggleChanged(bool isOn)
		{
			GameManager.Instance.SetGameSetting("5", isOn);
		}

		private void OnVibrateToggleChanged(bool isOn)
		{
			GameManager.Instance.SetGameSetting("6", isOn);
		}

		private void OnHighlightConnectedCellToggleChanged(bool isOn)
		{
			GameManager.Instance.SetGameSetting("7", isOn);
		}

		private void OnHighlightSameNumbersToggleChanged(bool isOn)
		{
			GameManager.Instance.SetGameSetting("8", isOn);
		}

		#endregion
	}
}
