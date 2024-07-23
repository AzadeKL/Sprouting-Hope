using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace UserInterfaceGridLayout
{
    public class FlexibleGridLayout : LayoutGroup
    {
        // Enum for the different types of fitting
        public enum FitType
        {
            Uniform,
            Width,
            Height,
            FixedRows,
            FixedColumns
        }

        // Serialized fields for the Inspector
        [Header("Flexible Grid")]
        public FitType fitType = FitType.Uniform;               // The type of fitting
        public int rows;                                        // The number of rows
        public int columns;                                     // The number of columns
        public Vector2 cellSize;                                // The size of each cell
        public Vector2 spacing;                                 // The spacing between cells

        // Booleans to determine if the grid should fit in the X and Y directions
        public bool fitX;
        public bool fitY;
        public bool keepCellsSquare;                            // Boolean to determine if cells should be kept square

        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------

        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();

            // If the fit type is Uniform, Width, or Height, calculate the number of rows and columns based on the square root of the number of children
            if (fitType == FitType.Width || fitType == FitType.Height || fitType == FitType.Uniform)
            {
                float squareRoot = Mathf.Sqrt(transform.childCount);
                rows = columns = Mathf.CeilToInt(squareRoot);
            }

            // If the fit type is Width or FixedColumns, calculate the number of rows based on the number of columns and children
            if (fitType == FitType.Width || fitType == FitType.FixedColumns)
            {
                rows = Mathf.CeilToInt(transform.childCount / (float)columns);
            }
            // If the fit type is Height or FixedRows, calculate the number of columns based on the number of rows and children
            if (fitType == FitType.Height || fitType == FitType.FixedRows)
            {
                columns = Mathf.CeilToInt(transform.childCount / (float)rows);
            }

            // Calculate the parent's width and height, subtracting the padding
            float parentWidth = rectTransform.rect.width - padding.left - padding.right;
            float parentHeight = rectTransform.rect.height - padding.top - padding.bottom;

            // Calculate the cell's width and height based on the parent's dimensions, the number of rows and columns, the spacing, and the padding
            float cellWidth = parentWidth / (float)columns - ((spacing.x / (float)columns) * (columns - 1));
            float cellHeight = parentHeight / (float)rows - ((spacing.y / (float)rows) * (rows - 1));

            // If fitX or fitY is true, set the cell's width or height to the calculated width or height
            cellSize.x = fitX ? cellWidth : cellSize.x;
            cellSize.y = fitY ? cellHeight : cellSize.y;

            // If keepCellsSquare is true, set the cell's height to its width
            if (keepCellsSquare)
            {
                cellSize.y = cellSize.x;
            }

            // Calculate the total width and height of the grid
            float totalWidth = cellSize.x * columns + spacing.x * (columns - 1);
            float totalHeight = cellSize.y * rows + spacing.y * (rows - 1);

            // Calculate the extra space available, subtracting the padding
            float extraWidth = rectTransform.rect.width - totalWidth - padding.left - padding.right;
            float extraHeight = rectTransform.rect.height - totalHeight - padding.top - padding.bottom;

            // Calculate the starting position of the grid based on the child alignment
            float startX = padding.left + extraWidth * ((int)childAlignment % 3) * 0.5f;
            float startY = padding.top + extraHeight * ((int)childAlignment / 3) * 0.5f;

            // Loop through each child and set its position and size
            int columnCount = 0;
            int rowCount = 0;

            for (int i = 0; i < rectChildren.Count; i++)
            {
                rowCount = i / columns;
                columnCount = i % columns;

                var item = rectChildren[i];

                var xPos = startX + (cellSize.x + spacing.x) * columnCount;
                var yPos = startY + (cellSize.y + spacing.y) * rowCount;

                SetChildAlongAxis(item, 0, xPos, cellSize.x);
                SetChildAlongAxis(item, 1, yPos, cellSize.y);
            }
        }

        // These methods are required by the LayoutGroup base class, but are not used in this script
        public override void CalculateLayoutInputVertical()
        {
        }

        public override void SetLayoutHorizontal()
        {
        }

        public override void SetLayoutVertical()
        {
        }
    }
}

