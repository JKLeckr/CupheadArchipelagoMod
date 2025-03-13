/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using UnityEngine;
using UnityEngine.UI;

namespace CupheadArchipelago.Unity {
    internal class APCore {
        public static readonly Color TEXT_COLOR = new Color(0.212f, 0.212f, 0.212f, 1f);
        public static readonly Color TEXT_SELECT_COLOR = new Color(0.676f, 0.212f, 0.212f, 1f);
        public static readonly Color TEXT_INACTIVE_COLOR = new Color(0.212f, 0.212f, 0.212f, 0.5f);

        public enum FontType {
            Bold,
            ExtraBold,
            MonoSpace
        }

        public static Text CreateSettingsTextComponent(GameObject obj, FontType type = FontType.Bold, TextAnchor alignment = TextAnchor.UpperLeft, bool wrap = false) {
            Text txt = obj.AddComponent<Text>();
            txt.color = TEXT_COLOR;
            txt.font = type switch {
                FontType.ExtraBold => FontLoader.GetFont(FontLoader.FontType.CupheadVogue_ExtraBold_merged),
                FontType.MonoSpace => FontLoader.GetFont(FontLoader.FontType.FGNewRetro),
                _ => FontLoader.GetFont(FontLoader.FontType.CupheadVogue_Bold_merged),
            };
            txt.fontSize = 32;
            txt.alignment = alignment;
            txt.horizontalOverflow = wrap ? HorizontalWrapMode.Wrap : HorizontalWrapMode.Overflow;
            return txt;
        }
    }
}
