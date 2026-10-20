using System.Collections.Generic;
using UnityEngine;

namespace Gorkha.Units
{
    public enum TacticalFormationType
    {
        Line,
        Box,
        Wedge,
        Staggered
    }

    /// <summary>
    /// Tactical RTS Army Formation Manager (Age of Empires style).
    /// Computes organized destination slots for groups of units moving together:
    /// - Line: Front rank wall of spearmen with archers and shock troops in rank
    /// - Box: Protective defensive square protecting central fragile units
    /// - Wedge: V-shaped shock charge formation for breakthrough
    /// - Staggered: Dispersed grid minimizing artillery/cannon splash damage
    /// </summary>
    public static class ArmyFormationController
    {
        public static Vector3[] CalculateFormationSlots(
            TacticalFormationType formation,
            List<UnitController> units,
            Vector3 targetCenter,
            Vector3 marchDirection,
            float spacing = 1.6f)
        {
            int count = units != null ? units.Count : 0;
            Vector3[] slots = new Vector3[count];
            if (count == 0) return slots;

            if (marchDirection.sqrMagnitude < 0.001f)
            {
                marchDirection = Vector3.forward;
            }
            else
            {
                marchDirection.y = 0;
                marchDirection.Normalize();
            }

            Vector3 rightVec = Vector3.Cross(Vector3.up, marchDirection).normalized;

            switch (formation)
            {
                case TacticalFormationType.Line:
                {
                    // 1. Line Formation: Wall of troops perpendicular to march vector
                    int unitsPerRank = Mathf.Min(count, 6);
                    for (int i = 0; i < count; i++)
                    {
                        int rank = i / unitsPerRank;
                        int col = i % unitsPerRank;
                        float xOffset = (col - (unitsPerRank - 1) * 0.5f) * spacing;
                        float zOffset = -rank * spacing * 1.25f;
                        slots[i] = targetCenter + rightVec * xOffset + marchDirection * zOffset;
                    }
                    break;
                }

                case TacticalFormationType.Wedge:
                {
                    // 2. Wedge (V-shape) Formation: Point unit at the front apex, wings tapering back
                    slots[0] = targetCenter + marchDirection * (spacing * 0.6f);
                    for (int i = 1; i < count; i++)
                    {
                        int row = (i + 1) / 2;
                        float side = (i % 2 == 1) ? -1f : 1f; // Alternating left and right wings
                        float xOffset = side * row * spacing * 0.95f;
                        float zOffset = -row * spacing * 1.15f;
                        slots[i] = targetCenter + rightVec * xOffset + marchDirection * zOffset;
                    }
                    break;
                }

                case TacticalFormationType.Box:
                {
                    // 3. Box Formation: Square grid protecting inner core
                    int sideLen = Mathf.CeilToInt(Mathf.Sqrt(count));
                    for (int i = 0; i < count; i++)
                    {
                        int row = i / sideLen;
                        int col = i % sideLen;
                        float xOffset = (col - (sideLen - 1) * 0.5f) * spacing;
                        float zOffset = -(row - (sideLen - 1) * 0.5f) * spacing;
                        slots[i] = targetCenter + rightVec * xOffset + marchDirection * zOffset;
                    }
                    break;
                }

                case TacticalFormationType.Staggered:
                default:
                {
                    // 4. Staggered Formation: Rows with alternate lateral shifts (AoE anti-catapult)
                    int cols = Mathf.Max(2, Mathf.CeilToInt(Mathf.Sqrt(count * 1.5f)));
                    for (int i = 0; i < count; i++)
                    {
                        int row = i / cols;
                        int col = i % cols;
                        float rowShift = (row % 2 == 1) ? spacing * 0.5f : 0f;
                        float xOffset = (col - (cols - 1) * 0.5f) * spacing + rowShift;
                        float zOffset = -row * spacing * 1.45f;
                        slots[i] = targetCenter + rightVec * xOffset + marchDirection * zOffset;
                    }
                    break;
                }
            }

            return slots;
        }
    }
}
