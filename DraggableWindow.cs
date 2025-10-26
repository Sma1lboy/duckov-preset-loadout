using UnityEngine;
using UnityEngine.EventSystems;

namespace PresetLoadout
{
    /// <summary>
    /// 可拖动窗口组件
    /// 添加到 GameObject 上使其可以通过鼠标拖动
    /// </summary>
    public class DraggableWindow : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private RectTransform? _rectTransform;
        private Canvas? _canvas;
        private Vector2 _dragOffset;

        void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvas = GetComponentInParent<Canvas>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            // 计算鼠标点击位置相对于窗口中心的偏移
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _rectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out _dragOffset
            );
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_rectTransform == null || _canvas == null)
                return;

            // 将屏幕坐标转换为 Canvas 坐标
            Vector2 localPoint;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvas.transform as RectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out localPoint))
            {
                // 更新窗口位置（减去拖动偏移，使窗口跟随鼠标）
                _rectTransform.anchoredPosition = localPoint - _dragOffset;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            // 拖动结束时可以添加额外逻辑（如限制窗口范围）
        }
    }
}
