using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Aptos.Unity.Sample.UI
{
    [ExecuteInEditMode]
    public class DynamicGridLayout : MonoBehaviour
    {
        public int cellNum;
        public float spacingCellPercentage;

        private float spaceing;

        void Start()
        {
            Resize();
        }

        private void Update()
        {
            Resize();
        }

        public void Resize()
        {
            float width = this.gameObject.GetComponent<RectTransform>().rect.width;

            spaceing = (width / cellNum) / spacingCellPercentage;
            width = width - ((cellNum - 1) * spaceing);

            Vector2 newSize = new Vector2(width / cellNum, width / cellNum);
            Vector2 newSpacing = new Vector2(spaceing, spaceing);

            this.gameObject.GetComponent<GridLayoutGroup>().cellSize = newSize;
            this.gameObject.GetComponent<GridLayoutGroup>().spacing = newSpacing;
        }
    }
}