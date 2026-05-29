using System.ComponentModel;

namespace PlayerBadge
{
    public class Config
    {
        [Description("Whether PlayerBadge is enabled. / 是否启用 PlayerBadge。")]
        public bool IsEnabled { get; set; } = true;

        [Description("Enable debug logging. / 是否启用调试日志。")]
        public bool Debug { get; set; } = false;

        [Description("User-facing language. Empty falls back to Chinese; use 'cn' for Chinese or 'en' for English. / 显示语言，留空回退中文；使用 'cn' 或 'en' 强制语言。")]
        public string Language { get; set; } = string.Empty;

        [Description("Custom title config file path. Empty stores PlayerBadge.txt in this plugin's LabAPI config folder. / 称号配置文件路径，留空则存储在本插件 LabAPI 配置目录。")]
        public string ConfigFilePath { get; set; } = string.Empty;

        [Description("Rainbow title color change interval in seconds. / 彩色称号颜色切换间隔秒数。")]
        public float RainbowInterval { get; set; } = 0.6f;
    }
}
