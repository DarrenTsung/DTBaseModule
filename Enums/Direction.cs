using DT;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DT {
	public enum Direction {
		UP = 1,
		RIGHT = 2,
		DOWN = 4,
		LEFT = 8
	}

	public static class DirectionExtensions {
		public static Vector2 Vector2Value(this Direction direction) {
			switch (direction) {
				case Direction.UP:
					return Vector2.up;
				case Direction.RIGHT:
					return Vector2.right;
				case Direction.DOWN:
					return -Vector2.up;
				case Direction.LEFT:
					return -Vector2.right;
			}
			return Vector2.up;
		}

		public static Direction OppositeDirection(this Direction direction) {
			switch (direction) {
				case Direction.UP:
					return Direction.DOWN;
				case Direction.RIGHT:
					return Direction.LEFT;
				case Direction.DOWN:
					return Direction.UP;
				case Direction.LEFT:
					return Direction.RIGHT;
			}
			return Direction.UP;
		}

		public static Direction ClockwiseDirection(this Direction direction) {
			switch (direction) {
				case Direction.UP:
					return Direction.RIGHT;
				case Direction.RIGHT:
					return Direction.DOWN;
				case Direction.DOWN:
					return Direction.LEFT;
				case Direction.LEFT:
					return Direction.UP;
			}
			return Direction.UP;
		}

		public static Direction CounterClockwiseDirection(this Direction direction) {
			switch (direction) {
				case Direction.UP:
					return Direction.LEFT;
				case Direction.RIGHT:
					return Direction.UP;
				case Direction.DOWN:
					return Direction.RIGHT;
				case Direction.LEFT:
					return Direction.DOWN;
			}
			return Direction.UP;
		}

		public static float ApplicableValueToVector2(this Direction direction, Vector2 input) {
			if (direction == Direction.LEFT || direction == Direction.RIGHT) {
				return input.x;
			} else {
				return input.y;
			}
		}

		public static bool ContainsDirection(this IList<Direction> directions, Direction direction) {
			if (directions == null) {
				return false;
			}

			for (int i = 0; i < directions.Count; i++) {
				Direction d = directions[i];
				if (d == direction) {
					return true;
				}
			}

			return false;
		}

		public static bool DoesNotContainDirection(this IList<Direction> directions, Direction direction) {
			return !directions.ContainsDirection(direction);
		}
	}

	public static class DirectionUtil {
		public static Direction ConvertVector2(Vector2 v) {
			if (Mathf.Abs(v.x) > Mathf.Abs(v.y)) {
				return (v.x > 0.0f) ? Direction.RIGHT : Direction.LEFT;
			} else {
				return (v.y > 0.0f) ? Direction.UP : Direction.DOWN;
			}
		}

		// Order-dependent. Does not return smallest angle between.
		// Ex. LEFT -> DOWN = 270
		public static int AngleInDegreesFrom(Direction a, Direction b) {
			int aValue = DirectionUtil.GetOrderedValue(a);
			int bValue = DirectionUtil.GetOrderedValue(b);

			int anglesBetween = (bValue - aValue) % 4;
			return anglesBetween * 90;
		}

		private static int GetOrderedValue(Direction d) {
			switch (d) {
				case Direction.UP:
					return 0;
				case Direction.RIGHT:
					return 1;
				case Direction.DOWN:
					return 2;
				case Direction.LEFT:
				default:
					return 3;
			}
		}
	}
}
