using Nez.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.UI.Elements
{
    public class BulletPointSelector : Table, IGamepadFocusable
    {
        Label _asteriskLabel;
        Label _choiceLabel;

        public event Action<string> OnSelected;

        public BulletPointSelector(string choiceText, Skin skin, string styleName = null)
        {
            _asteriskLabel = new Label("*", skin, styleName);
            Add(_asteriskLabel).SetSpaceRight(10f);
            _choiceLabel = new Label(choiceText, skin, styleName);
            Add(_choiceLabel);

            _asteriskLabel.SetVisible(false);
        }

        #region IGamepadFocusable

        public bool ShouldUseExplicitFocusableControl { get; set; }
        public IGamepadFocusable GamepadUpElement { get; set; }
        public IGamepadFocusable GamepadDownElement { get; set; }
        public IGamepadFocusable GamepadLeftElement { get; set; }
        public IGamepadFocusable GamepadRightElement { get; set; }

        public void EnableExplicitFocusableControl(IGamepadFocusable upEle, IGamepadFocusable downEle, IGamepadFocusable leftEle, IGamepadFocusable rightEle)
        {
            ShouldUseExplicitFocusableControl = true;
            GamepadUpElement = upEle;
            GamepadDownElement = downEle;
            GamepadLeftElement = leftEle;
            GamepadRightElement = rightEle;
        }

        public void OnActionButtonPressed()
        {
            Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds.Menu_select);
            OnSelected?.Invoke(_choiceLabel.GetText());
            GetStage().SetGamepadFocusElement(null);
        }

        public void OnActionButtonReleased()
        {
            //throw new NotImplementedException();
        }

        public void OnFocused()
        {
            Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds._002_Hover_02);
            _asteriskLabel.SetVisible(true);
            //SetText($"* {_givenText}");

            //if (OnLabelFocused != null)
            //    OnLabelFocused();
            //unfocus other focus labels
            //var focusLabels = GetStage().FindAllElementsOfType<FocusLabel>();
            //focusLabels.Remove(this);
            //foreach(var focusLabel in focusLabels)
            //{
            //    focusLabel.OnUnfocused();
            //}
        }

        public void OnUnfocused()
        {
            _asteriskLabel.SetVisible(false);
            //SetText($"  {_givenText}");
        }

        public void OnUnhandledDirectionPressed(Direction direction)
        {
            //throw new NotImplementedException();
        }

        #endregion
    }
}
