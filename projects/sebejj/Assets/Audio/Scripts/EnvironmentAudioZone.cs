using UnityEngine;

namespace SebeJJ.Audio
{
    /// <summary>
    /// 环境音效区域 - 用于触发不同深度的混响设置
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class EnvironmentAudioZone : MonoBehaviour
    {
        public enum ZoneType
        {
            Base,           // 基地内部
            ShallowWater,   // 浅海
            DeepWater,      // 深海
            Cave            // 洞穴
        }

        [Header("区域设置")]
        [SerializeField] private ZoneType zoneType = ZoneType.ShallowWater;
        [SerializeField] private float transitionTime = 2.0f;

        [Header("混响参数")]
        [SerializeField] private AudioReverbZone reverbZone;

        [Header("背景音乐")]
        [SerializeField] private bool changeMusic = true;
        [SerializeField] private MusicType targetMusic;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                ApplyZoneSettings();
            }
        }

        private void ApplyZoneSettings()
        {
            // 应用混响设置
            if (reverbZone != null)
            {
                ApplyReverbSettings();
            }

            // 切换背景音乐
            if (changeMusic && AudioManager.Instance != null)
            {
                AudioManager.Instance.TransitionToMusic(targetMusic, transitionTime);
            }
        }

        private void ApplyReverbSettings()
        {
            switch (zoneType)
            {
                case ZoneType.Base:
                    // 基地内部 - 干燥环境
                    reverbZone.room = -500;
                    reverbZone.roomHF = -200;
                    reverbZone.roomLF = 0;
                    reverbZone.decayTime = 1.5f;
                    reverbZone.decayHFRatio = 0.8f;
                    reverbZone.reflections = -300;
                    reverbZone.reflectionsDelay = 0.02f;
                    reverbZone.reverb = -200;
                    reverbZone.reverbDelay = 0.03f;
                    reverbZone.diffusion = 90;
                    reverbZone.density = 85;
                    reverbZone.hfReference = 5000;
                    reverbZone.lfReference = 250;
                    break;

                case ZoneType.ShallowWater:
                    // 浅海 - 轻微混响
                    reverbZone.room = -1000;
                    reverbZone.roomHF = -400;
                    reverbZone.roomLF = 0;
                    reverbZone.decayTime = 2.5f;
                    reverbZone.decayHFRatio = 0.5f;
                    reverbZone.reflections = -1000;
                    reverbZone.reflectionsDelay = 0.1f;
                    reverbZone.reverb = -500;
                    reverbZone.reverbDelay = 0.05f;
                    reverbZone.diffusion = 80;
                    reverbZone.density = 70;
                    reverbZone.hfReference = 5000;
                    reverbZone.lfReference = 250;
                    break;

                case ZoneType.DeepWater:
                    // 深海 - 强烈混响，高频衰减
                    reverbZone.room = -1500;
                    reverbZone.roomHF = -800;
                    reverbZone.roomLF = -200;
                    reverbZone.decayTime = 4.0f;
                    reverbZone.decayHFRatio = 0.3f;
                    reverbZone.reflections = -1500;
                    reverbZone.reflectionsDelay = 0.2f;
                    reverbZone.reverb = -800;
                    reverbZone.reverbDelay = 0.1f;
                    reverbZone.diffusion = 60;
                    reverbZone.density = 50;
                    reverbZone.hfReference = 3000;
                    reverbZone.lfReference = 150;
                    break;

                case ZoneType.Cave:
                    // 洞穴 - 长混响
                    reverbZone.room = -800;
                    reverbZone.roomHF = -600;
                    reverbZone.roomLF = -100;
                    reverbZone.decayTime = 5.0f;
                    reverbZone.decayHFRatio = 0.4f;
                    reverbZone.reflections = -1200;
                    reverbZone.reflectionsDelay = 0.15f;
                    reverbZone.reverb = -600;
                    reverbZone.reverbDelay = 0.08f;
                    reverbZone.diffusion = 70;
                    reverbZone.density = 60;
                    reverbZone.hfReference = 4000;
                    reverbZone.lfReference = 200;
                    break;
            }
        }

        private void OnDrawGizmos()
        {
            // 绘制区域可视化
            Collider col = GetComponent<Collider>();
            if (col == null) return;

            Color gizmoColor = zoneType switch
            {
                ZoneType.Base => new Color(0, 1, 0, 0.3f),
                ZoneType.ShallowWater => new Color(0, 0.5f, 1, 0.3f),
                ZoneType.DeepWater => new Color(0, 0, 0.8f, 0.3f),
                ZoneType.Cave => new Color(0.3f, 0.2f, 0.1f, 0.3f),
                _ => Color.gray
            };

            Gizmos.color = gizmoColor;
            Gizmos.DrawCube(col.bounds.center, col.bounds.size);

            // 绘制标签
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(col.bounds.center, $"AudioZone: {zoneType}");
            #endif
        }
    }
}
