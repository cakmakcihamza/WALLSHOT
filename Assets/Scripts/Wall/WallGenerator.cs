using UnityEngine;
using WallBreaker.Game;

namespace WallBreaker.Wall
{
    public class WallGenerator : MonoBehaviour
    {
        [Header("Wall Size")]
        [SerializeField] private int columns = 5;
        [SerializeField] private int rows = 6;

        [Header("Brick Size")]
        [SerializeField] private float brickWidth = 1f;
        [SerializeField] private float brickHeight = 0.45f;
        [SerializeField] private float brickDepth = 0.35f;
        [SerializeField] private float spacing = 0.03f;

        [Header("Physics")]
        [SerializeField] private float brickMass = 1f;
        [SerializeField] private float activationImpulse = 0.2f;

        private WallLayoutPattern layoutPattern;
        private CannonLayoutPattern cannonLayoutPattern;
        private CrossbowLayoutPattern crossbowLayoutPattern;
        private BazookaLayoutPattern bazookaLayoutPattern;
        private bool useCannonLayout;
        private bool useCrossbowLayout;
        private bool useBazookaLayout;
        private bool useDesertEagleLayout;
        private int desertEagleLevel;

        private enum WallLayoutPattern
        {
            SolidRectangle,
            CenterGap,
            TaperedTower,
            ArchGate,
            CheckerStaggered,
            DoubleTower,
            Fortress,
            DiamondPyramid,
            ZigZagWall,
            FinalFort
        }

        private enum CannonLayoutPattern
        {
            ReinforcedWall,
            TwinTowersLowCenter,
            SteppedFortress,
            HeavyGateFortress,
            TripleTowers,
            AlternatingColumns,
            CastleKeep,
            TopLoadedCollapse,
            AsymmetricFortress,
            FinalCannonFortress
        }

        private enum CrossbowLayoutPattern
        {
            PrecisionPosts,
            HighTargets,
            WindowWall,
            ThinTowers,
            BridgeSupports,
            SplitTargets,
            PrecisionFortress,
            ElevatedPlatforms,
            WeakPointChallenge,
            CrossbowFinalChallenge
        }

        private enum BazookaLayoutPattern
        {
            BlastWall,
            BlastTowers,
            DemolitionBlocks,
            ReinforcedGate,
            ChainReaction,
            HeavyBunkers,
            BlastCastle,
            CollapseFortress,
            DemolitionFortress,
            FinalBlastFortress
        }

        private void Start()
        {
            ApplyLevelVariation(WeaponManager.CurrentStage, LevelManager.CurrentLevel);
            GenerateWall();
            if (WeaponManager.CurrentStage == WeaponStage.Slingshot && LevelManager.CurrentLevel == 6)
            {
                LevelObstacle.EnsureForWall(transform);
            }
            else if (WeaponManager.CurrentStage == WeaponStage.Slingshot && LevelManager.CurrentLevel == 7)
            {
                RotatingObstacle.EnsureRotatingForWall(transform);
            }
            else if (WeaponManager.CurrentStage == WeaponStage.Slingshot && LevelManager.CurrentLevel == 8)
            {
                MovingGapObstacle.EnsureMovingForWall(transform);
            }
            else if (WeaponManager.CurrentStage == WeaponStage.Slingshot && LevelManager.CurrentLevel == 9)
            {
                VerticalMovingGapObstacle.EnsureVerticalForWall(transform);
            }
            else if (WeaponManager.CurrentStage == WeaponStage.Slingshot && LevelManager.CurrentLevel == 10)
            {
                RotatingObstacle.EnsureFinalRotorsForWall(transform);
            }
            else if (WeaponManager.CurrentStage == WeaponStage.DesertEagle && LevelManager.CurrentLevel == 6)
            {
                LevelObstacle.EnsureDesertEagleForWall(transform);
            }
            else if (WeaponManager.CurrentStage == WeaponStage.DesertEagle && LevelManager.CurrentLevel == 7)
            {
                RotatingObstacle.EnsureDesertEagleRotorForWall(transform);
            }
            else if (WeaponManager.CurrentStage == WeaponStage.DesertEagle && LevelManager.CurrentLevel == 8)
            {
                MovingGapObstacle.EnsureDesertEagleMovingForWall(transform);
            }
            else if (WeaponManager.CurrentStage == WeaponStage.DesertEagle && LevelManager.CurrentLevel == 9)
            {
                VerticalMovingGapObstacle.EnsureDesertEagleVerticalForWall(transform);
            }
            else if (WeaponManager.CurrentStage == WeaponStage.DesertEagle && LevelManager.CurrentLevel == 10)
            {
                RotatingObstacle.EnsureDesertEagleFinalRotorsForWall(transform);
            }
            else if (WeaponManager.CurrentStage == WeaponStage.Cannon && LevelManager.CurrentLevel == 6)
            {
                LevelObstacle.EnsureCannonForWall(transform);
            }
            else if (WeaponManager.CurrentStage == WeaponStage.Cannon && LevelManager.CurrentLevel == 7)
            {
                RotatingObstacle.EnsureCannonRotorForWall(transform);
            }
            else if (WeaponManager.CurrentStage == WeaponStage.Cannon && LevelManager.CurrentLevel == 8)
            {
                MovingGapObstacle.EnsureCannonMovingForWall(transform);
            }
            else if (WeaponManager.CurrentStage == WeaponStage.Cannon && LevelManager.CurrentLevel == 9)
            {
                VerticalMovingGapObstacle.EnsureCannonVerticalForWall(transform);
            }
            else if (WeaponManager.CurrentStage == WeaponStage.Cannon && LevelManager.CurrentLevel == 10)
            {
                RotatingObstacle.EnsureCannonFinalRotorsForWall(transform);
            }
            else if (WeaponManager.CurrentStage == WeaponStage.Crossbow || WeaponManager.CurrentStage == WeaponStage.Bazooka)
            {
                bool bazooka = WeaponManager.CurrentStage == WeaponStage.Bazooka;
                switch (LevelManager.CurrentLevel)
                {
                    case 6: LevelObstacle.EnsureRemainingFrame(transform, bazooka); break;
                    case 7: RotatingObstacle.EnsureRemainingRotors(transform, bazooka, false); break;
                    case 8: MovingGapObstacle.EnsureRemainingMoving(transform, bazooka); break;
                    case 9: VerticalMovingGapObstacle.EnsureRemainingVertical(transform, bazooka); break;
                    case 10: RotatingObstacle.EnsureRemainingRotors(transform, bazooka, true); break;
                }
            }
        }

        private void ApplyLevelVariation(WeaponStage weaponStage, int level)
        {
            useDesertEagleLayout = weaponStage == WeaponStage.DesertEagle;
            if (useDesertEagleLayout)
            {
                desertEagleLevel = Mathf.Clamp(level, 1, 10);
                useCannonLayout = useCrossbowLayout = useBazookaLayout = false;
                columns = desertEagleLevel <= 4 ? 5 : 7;
                rows = desertEagleLevel switch
                {
                    1 => 2,
                    2 => 3,
                    3 => 4,
                    4 => 4,
                    5 => 4,
                    6 => 5,
                    7 => 5,
                    8 => 6,
                    9 => 6,
                    _ => 7
                };
                return;
            }

            if (weaponStage == WeaponStage.Cannon)
            {
                ApplyCannonLevelVariation(level);
                return;
            }

            if (weaponStage == WeaponStage.Crossbow)
            {
                ApplyCrossbowLevelVariation(level);
                return;
            }

            if (weaponStage == WeaponStage.Bazooka)
            {
                ApplyBazookaLevelVariation(level);
                return;
            }

            ApplySlingshotLevelVariation(level);
        }

        private void ApplySlingshotLevelVariation(int level)
        {
            useCannonLayout = false;
            useCrossbowLayout = false;
            useBazookaLayout = false;
            layoutPattern = WallLayoutPattern.SolidRectangle;

            if (level <= 1)
            {
                columns = 5;
                rows = 6;
                return;
            }

            if (level == 2)
            {
                columns = 6;
                rows = 6;
                layoutPattern = WallLayoutPattern.CenterGap;
                return;
            }

            if (level == 3)
            {
                columns = 5;
                rows = 7;
                layoutPattern = WallLayoutPattern.TaperedTower;
                return;
            }

            if (level == 4)
            {
                columns = 5;
                rows = 6;
                layoutPattern = WallLayoutPattern.ArchGate;
                return;
            }

            if (level == 5)
            {
                columns = 6;
                rows = 6;
                layoutPattern = WallLayoutPattern.CheckerStaggered;
                return;
            }

            if (level == 6)
            {
                columns = 7;
                rows = 6;
                layoutPattern = WallLayoutPattern.DoubleTower;
                return;
            }

            if (level == 7)
            {
                columns = 7;
                rows = 6;
                layoutPattern = WallLayoutPattern.Fortress;
                return;
            }

            if (level == 8)
            {
                columns = 7;
                rows = 6;
                layoutPattern = WallLayoutPattern.DiamondPyramid;
                return;
            }

            if (level == 9)
            {
                columns = 6;
                rows = 6;
                layoutPattern = WallLayoutPattern.ZigZagWall;
                return;
            }

            columns = 7;
            rows = 6;
            layoutPattern = WallLayoutPattern.FinalFort;
        }

        private void ApplyCannonLevelVariation(int level)
        {
            useCannonLayout = true;
            useCrossbowLayout = false;
            useBazookaLayout = false;
            columns = 7;
            rows = 6;
            cannonLayoutPattern = CannonLayoutPattern.ReinforcedWall;

            if (level <= 1)
            {
                cannonLayoutPattern = CannonLayoutPattern.ReinforcedWall;
                return;
            }

            if (level == 2)
            {
                cannonLayoutPattern = CannonLayoutPattern.TwinTowersLowCenter;
                return;
            }

            if (level == 3)
            {
                cannonLayoutPattern = CannonLayoutPattern.SteppedFortress;
                return;
            }

            if (level == 4)
            {
                cannonLayoutPattern = CannonLayoutPattern.HeavyGateFortress;
                return;
            }

            if (level == 5)
            {
                cannonLayoutPattern = CannonLayoutPattern.TripleTowers;
                return;
            }

            if (level == 6)
            {
                cannonLayoutPattern = CannonLayoutPattern.AlternatingColumns;
                return;
            }

            if (level == 7)
            {
                rows = 7;
                cannonLayoutPattern = CannonLayoutPattern.CastleKeep;
                return;
            }

            if (level == 8)
            {
                cannonLayoutPattern = CannonLayoutPattern.TopLoadedCollapse;
                return;
            }

            if (level == 9)
            {
                cannonLayoutPattern = CannonLayoutPattern.AsymmetricFortress;
                return;
            }

            rows = 7;
            cannonLayoutPattern = CannonLayoutPattern.FinalCannonFortress;
        }

        private void ApplyCrossbowLevelVariation(int level)
        {
            useCannonLayout = false;
            useCrossbowLayout = true;
            useBazookaLayout = false;
            columns = 7;
            rows = 6;
            crossbowLayoutPattern = CrossbowLayoutPattern.PrecisionPosts;

            if (level <= 1)
            {
                crossbowLayoutPattern = CrossbowLayoutPattern.PrecisionPosts;
                return;
            }

            if (level == 2)
            {
                crossbowLayoutPattern = CrossbowLayoutPattern.HighTargets;
                return;
            }

            if (level == 3)
            {
                crossbowLayoutPattern = CrossbowLayoutPattern.WindowWall;
                return;
            }

            if (level == 4)
            {
                rows = 7;
                crossbowLayoutPattern = CrossbowLayoutPattern.ThinTowers;
                return;
            }

            if (level == 5)
            {
                crossbowLayoutPattern = CrossbowLayoutPattern.BridgeSupports;
                return;
            }

            if (level == 6)
            {
                crossbowLayoutPattern = CrossbowLayoutPattern.SplitTargets;
                return;
            }

            if (level == 7)
            {
                rows = 7;
                crossbowLayoutPattern = CrossbowLayoutPattern.PrecisionFortress;
                return;
            }

            if (level == 8)
            {
                rows = 7;
                crossbowLayoutPattern = CrossbowLayoutPattern.ElevatedPlatforms;
                return;
            }

            if (level == 9)
            {
                rows = 7;
                crossbowLayoutPattern = CrossbowLayoutPattern.WeakPointChallenge;
                return;
            }

            rows = 7;
            crossbowLayoutPattern = CrossbowLayoutPattern.CrossbowFinalChallenge;
        }

        private void ApplyBazookaLevelVariation(int level)
        {
            useCannonLayout = false;
            useCrossbowLayout = false;
            useBazookaLayout = true;
            columns = 7;
            rows = 6;
            bazookaLayoutPattern = BazookaLayoutPattern.BlastWall;

            if (level <= 1)
            {
                bazookaLayoutPattern = BazookaLayoutPattern.BlastWall;
                return;
            }

            if (level == 2)
            {
                bazookaLayoutPattern = BazookaLayoutPattern.BlastTowers;
                return;
            }

            if (level == 3)
            {
                bazookaLayoutPattern = BazookaLayoutPattern.DemolitionBlocks;
                return;
            }

            if (level == 4)
            {
                bazookaLayoutPattern = BazookaLayoutPattern.ReinforcedGate;
                return;
            }

            if (level == 5)
            {
                bazookaLayoutPattern = BazookaLayoutPattern.ChainReaction;
                return;
            }

            if (level == 6)
            {
                bazookaLayoutPattern = BazookaLayoutPattern.HeavyBunkers;
                return;
            }

            if (level == 7)
            {
                rows = 7;
                bazookaLayoutPattern = BazookaLayoutPattern.BlastCastle;
                return;
            }

            if (level == 8)
            {
                rows = 7;
                bazookaLayoutPattern = BazookaLayoutPattern.CollapseFortress;
                return;
            }

            if (level == 9)
            {
                rows = 7;
                bazookaLayoutPattern = BazookaLayoutPattern.DemolitionFortress;
                return;
            }

            rows = 7;
            bazookaLayoutPattern = BazookaLayoutPattern.FinalBlastFortress;
        }

        private void GenerateWall()
        {
            if (useDesertEagleLayout)
            {
                GenerateDesertEagleWall();
                return;
            }

            float totalWidth = columns * brickWidth + (columns - 1) * spacing;
            Vector3 bottomLeftOffset = new Vector3(
                -totalWidth * 0.5f + brickWidth * 0.5f,
                brickHeight * 0.5f,
                0f
            );

            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    if (ShouldSkipBrick(row, column))
                    {
                        continue;
                    }

                    Vector3 localPosition = bottomLeftOffset + new Vector3(
                        column * (brickWidth + spacing),
                        row * (brickHeight + spacing),
                        0f
                    );

                    CreateBrick(row, column, transform.TransformPoint(localPosition));

                    if (ShouldCreateBazookaBackLayerBrick(row, column))
                    {
                        Vector3 backLayerPosition = localPosition + new Vector3(0f, 0f, brickDepth + spacing);
                        CreateBrick(row, column, transform.TransformPoint(backLayerPosition), "_Back");
                    }
                }
            }
        }

        private void GenerateDesertEagleWall()
        {
            // Fit seven columns inside the existing aim span without changing legacy brick sizes.
            float width = Mathf.Min(brickWidth, 0.85f);
            float height = Mathf.Min(brickHeight, 0.45f);
            float depth = Mathf.Min(brickDepth, 0.30f);
            float totalWidth = columns * width + (columns - 1) * spacing;
            int layers = 1;
            for (int layer = 0; layer < layers; layer++)
            {
                for (int row = 0; row < rows; row++)
                {
                    for (int column = 0; column < columns; column++)
                    {
                        if (!ShouldKeepDesertEagleBrick(row, column, layer)) continue;
                        Vector3 localPosition = new Vector3(
                            -totalWidth * 0.5f + width * 0.5f + column * (width + spacing),
                            height * 0.5f + row * (height + spacing),
                            layer * (depth + spacing));
                        // Reuse the same physics/activation factory and only size the new brick.
                        CreateBrick(row, column, transform.TransformPoint(localPosition), layer == 0 ? "" : "_Back");
                        Transform brick = transform.GetChild(transform.childCount - 1);
                        brick.localScale = new Vector3(width, height, depth);
                    }
                }
            }
        }

        private bool ShouldKeepDesertEagleBrick(int row, int column, int layer)
        {
            switch (desertEagleLevel)
            {
                case 1: // Ten bricks: an accessible first wall, even without chain reactions.
                    return true;
                case 2: // Central window, with a lintel above it.
                    return !(column == 2 && row == 1);
                case 3: // Two separate narrow towers.
                    return column == 1 || column == 3;
                case 4: // Two exposed feet under a wider upper wall.
                    return row == 0 ? column == 1 || column == 3 : column >= 1 && column <= 3;
                case 5: // Seven, five, three, one: a centered pyramid.
                    return Mathf.Abs(column - 3) <= 3 - row;
                case 6: // Three columns, each with a single-brick footprint.
                    return column == 1 || column == 3 || column == 5;
                case 7: // Narrow central passage, capped by a top lintel.
                    return row == 4 ? column >= 1 && column <= 5 : column == 1 || column == 2 || column == 4 || column == 5;
                case 8: // Narrow neck supporting a broad, heavier upper section.
                    return row <= 2 ? column == 3 : column >= 1 && column <= 5;
                case 9: // Three support groups joined by upper beams.
                    return row <= 2 ? column == 1 || column == 3 || column == 5 :
                        row == 3 || row == 5 ? column >= 1 && column <= 5 : column == 2 || column == 4;
                case 10: // Original final silhouette, retaining only its front layer.
                    if (layer != 0) return false;
                    if (row <= 1) return column == 1 || column == 3 || column == 5;
                    if (row == 6) return column >= 2 && column <= 4;
                    return column >= 1 && column <= 5 && !(row == 3 && column == 3);
                default:
                    return false;
            }
        }

        private bool ShouldSkipBrick(int row, int column)
        {
            if (useCannonLayout)
            {
                return ShouldSkipCannonBrick(row, column);
            }

            if (useCrossbowLayout)
            {
                return ShouldSkipCrossbowBrick(row, column);
            }

            if (useBazookaLayout)
            {
                return ShouldSkipBazookaBrick(row, column);
            }

            if (layoutPattern == WallLayoutPattern.CenterGap)
            {
                bool isMiddleColumn = column == 2 || column == 3;
                bool isMiddleRow = row == 2 || row == 3;
                return isMiddleColumn && isMiddleRow;
            }

            if (layoutPattern == WallLayoutPattern.TaperedTower)
            {
                if (row <= 2)
                {
                    return false;
                }

                if (row <= 5)
                {
                    return column == 0 || column == columns - 1;
                }

                return column != columns / 2;
            }

            if (layoutPattern == WallLayoutPattern.ArchGate)
            {
                bool isGateOpening = row <= 2 && column >= 1 && column <= 3;
                bool isArchShoulderGap = row == 3 && column == 2;
                return isGateOpening || isArchShoulderGap;
            }

            if (layoutPattern == WallLayoutPattern.CheckerStaggered)
            {
                if (row == 0 || row == rows - 1)
                {
                    return false;
                }

                bool skipEvenRowSide = row % 2 == 0 && (column == 1 || column == 4);
                bool skipOddRowCenter = row % 2 == 1 && (column == 2 || column == 3);
                return skipEvenRowSide || skipOddRowCenter;
            }

            if (layoutPattern == WallLayoutPattern.DoubleTower)
            {
                return column == 3;
            }

            if (layoutPattern == WallLayoutPattern.Fortress)
            {
                bool isGateOpening = row >= 1 && row <= 3 && column == 3;
                bool isUpperSideCut = row == 5 && (column == 0 || column == 1 || column == 5 || column == 6);
                return isGateOpening || isUpperSideCut;
            }

            if (layoutPattern == WallLayoutPattern.DiamondPyramid)
            {
                int bricksInRow = row switch
                {
                    0 => 7,
                    1 => 5,
                    2 => 5,
                    3 => 3,
                    4 => 3,
                    _ => 1
                };

                int sideSkip = (columns - bricksInRow) / 2;
                return column < sideSkip || column >= columns - sideSkip;
            }

            if (layoutPattern == WallLayoutPattern.ZigZagWall)
            {
                return row switch
                {
                    0 => false,
                    1 => column == 0,
                    2 => column == 5,
                    3 => column == 0 || column == 1,
                    4 => column == 4 || column == 5,
                    _ => column == 0 || column == 5
                };
            }

            if (layoutPattern == WallLayoutPattern.FinalFort)
            {
                return row switch
                {
                    0 => false,
                    1 => column == 3,
                    2 => column >= 2 && column <= 4,
                    3 => false,
                    4 => column == 0 || column == 6,
                    _ => column == 0 || column == 1 || column == 5 || column == 6
                };
            }

            return false;
        }

        private bool ShouldSkipCrossbowBrick(int row, int column)
        {
            bool shouldKeep = crossbowLayoutPattern switch
            {
                CrossbowLayoutPattern.PrecisionPosts => ShouldKeepCrossbowPrecisionPosts(row, column),
                CrossbowLayoutPattern.HighTargets => ShouldKeepCrossbowHighTargets(row, column),
                CrossbowLayoutPattern.WindowWall => ShouldKeepCrossbowWindowWall(row, column),
                CrossbowLayoutPattern.ThinTowers => ShouldKeepCrossbowThinTowers(row, column),
                CrossbowLayoutPattern.BridgeSupports => ShouldKeepCrossbowBridgeSupports(row, column),
                CrossbowLayoutPattern.SplitTargets => ShouldKeepCrossbowSplitTargets(row, column),
                CrossbowLayoutPattern.PrecisionFortress => ShouldKeepCrossbowPrecisionFortress(row, column),
                CrossbowLayoutPattern.ElevatedPlatforms => ShouldKeepCrossbowElevatedPlatforms(row, column),
                CrossbowLayoutPattern.WeakPointChallenge => ShouldKeepCrossbowWeakPointChallenge(row, column),
                CrossbowLayoutPattern.CrossbowFinalChallenge => ShouldKeepCrossbowFinalChallenge(row, column),
                _ => true
            };

            return !shouldKeep;
        }

        private bool ShouldKeepCrossbowPrecisionPosts(int row, int column)
        {
            bool post = column == 1 || column == 3 || column == 5;
            bool highTarget = row == 5 && (column == 2 || column == 4);
            return post || highTarget;
        }

        private bool ShouldKeepCrossbowHighTargets(int row, int column)
        {
            bool lowerSupports = row <= 2 && (column == 1 || column == 3 || column == 5);
            bool elevatedTargets = row >= 4 && (column == 0 || column == 2 || column == 4 || column == 6);
            bool centerMarker = row == 3 && column == 3;
            return lowerSupports || elevatedTargets || centerMarker;
        }

        private bool ShouldKeepCrossbowWindowWall(int row, int column)
        {
            bool frame = row == 0 || row == 5 || column == 0 || column == 6;
            bool innerPillars = column == 2 || column == 4;
            bool windowGap = row >= 2 && row <= 3 && column >= 2 && column <= 4;
            return (frame || innerPillars) && !windowGap;
        }

        private bool ShouldKeepCrossbowThinTowers(int row, int column)
        {
            bool tower = column == 0 || column == 2 || column == 4 || column == 6;
            bool cap = row == 6 && (column == 1 || column == 3 || column == 5);
            bool baseBrace = row == 0 && (column == 1 || column == 5);
            return tower || cap || baseBrace;
        }

        private bool ShouldKeepCrossbowBridgeSupports(int row, int column)
        {
            bool supports = row <= 2 && (column == 1 || column == 5);
            bool bridge = row == 4 && column >= 1 && column <= 5;
            bool upperTargets = row == 5 && (column == 2 || column == 4);
            bool weakCenter = row == 3 && column == 3;
            return supports || bridge || upperTargets || weakCenter;
        }

        private bool ShouldKeepCrossbowSplitTargets(int row, int column)
        {
            bool leftCluster = column <= 1 && row <= 4;
            bool rightCluster = column >= 5 && row <= 4;
            bool centerHigh = column == 3 && row >= 2;
            bool sideHighTargets = row == 5 && (column == 2 || column == 4);
            return leftCluster || rightCluster || centerHigh || sideHighTargets;
        }

        private bool ShouldKeepCrossbowPrecisionFortress(int row, int column)
        {
            bool outerFrame = column == 0 || column == 6 || row == 0 || row == 6;
            bool narrowSlots = row >= 2 && row <= 4 && (column == 2 || column == 4);
            bool gateGap = row <= 2 && column == 3;
            bool upperWeakPoint = row == 5 && column == 3;
            return (outerFrame || narrowSlots || upperWeakPoint) && !gateGap;
        }

        private bool ShouldKeepCrossbowElevatedPlatforms(int row, int column)
        {
            bool lowSupports = row <= 2 && (column == 1 || column == 3 || column == 5);
            bool platforms = (row == 4 || row == 6) && (column >= 1 && column <= 5);
            bool platformGap = row == 4 && column == 3;
            bool topPrecisionTargets = row == 5 && (column == 0 || column == 6);
            return (lowSupports || platforms || topPrecisionTargets) && !platformGap;
        }

        private bool ShouldKeepCrossbowWeakPointChallenge(int row, int column)
        {
            bool baseClusters = row <= 1 && (column <= 2 || column >= 4);
            bool weakSupports = row >= 2 && row <= 3 && (column == 2 || column == 4);
            bool heavyTargets = row >= 4 && row <= 5 && column >= 1 && column <= 5;
            bool topWeakPoint = row == 6 && column == 3;
            bool centerGap = row == 4 && column == 3;
            return (baseClusters || weakSupports || heavyTargets || topWeakPoint) && !centerGap;
        }

        private bool ShouldKeepCrossbowFinalChallenge(int row, int column)
        {
            bool lowerNeedles = row <= 2 && (column == 0 || column == 2 || column == 4 || column == 6);
            bool midWindows = row >= 3 && row <= 4 && (column == 1 || column == 3 || column == 5);
            bool upperBridge = row == 5 && column >= 1 && column <= 5;
            bool finalTargets = row == 6 && (column == 2 || column == 3 || column == 4);
            bool precisionGap = row == 5 && column == 3;
            return (lowerNeedles || midWindows || upperBridge || finalTargets) && !precisionGap;
        }

        private bool ShouldSkipBazookaBrick(int row, int column)
        {
            bool shouldKeep = bazookaLayoutPattern switch
            {
                BazookaLayoutPattern.BlastWall => ShouldKeepBazookaBlastWall(row, column),
                BazookaLayoutPattern.BlastTowers => ShouldKeepBazookaBlastTowers(row, column),
                BazookaLayoutPattern.DemolitionBlocks => ShouldKeepBazookaDemolitionBlocks(row, column),
                BazookaLayoutPattern.ReinforcedGate => ShouldKeepBazookaReinforcedGate(row, column),
                BazookaLayoutPattern.ChainReaction => ShouldKeepBazookaChainReaction(row, column),
                BazookaLayoutPattern.HeavyBunkers => ShouldKeepBazookaHeavyBunkers(row, column),
                BazookaLayoutPattern.BlastCastle => ShouldKeepBazookaBlastCastle(row, column),
                BazookaLayoutPattern.CollapseFortress => ShouldKeepBazookaCollapseFortress(row, column),
                BazookaLayoutPattern.DemolitionFortress => ShouldKeepBazookaDemolitionFortress(row, column),
                BazookaLayoutPattern.FinalBlastFortress => ShouldKeepBazookaFinalBlastFortress(row, column),
                _ => true
            };

            return !shouldKeep;
        }

        private bool ShouldKeepBazookaBlastWall(int row, int column)
        {
            bool denseCore = row <= 4 && column >= 1 && column <= 5;
            bool sideCaps = row >= 1 && row <= 3 && (column == 0 || column == 6);
            bool topTargets = row == 5 && column >= 2 && column <= 4;
            return denseCore || sideCaps || topTargets;
        }

        private bool ShouldKeepBazookaBlastTowers(int row, int column)
        {
            bool leftTower = column <= 2 && row <= 5;
            bool rightTower = column >= 4 && row <= 5;
            bool centerRubble = row <= 2 && column == 3;
            bool weakBaseCut = row == 1 && (column == 1 || column == 5);
            return (leftTower || rightTower || centerRubble) && !weakBaseCut;
        }

        private bool ShouldKeepBazookaDemolitionBlocks(int row, int column)
        {
            bool leftBlock = column <= 1 && row <= 3;
            bool centerBlock = column >= 3 && column <= 4 && row >= 1 && row <= 5;
            bool rightBlock = column >= 5 && row <= 4;
            bool basePads = row == 0 && (column == 2 || column == 5);
            return leftBlock || centerBlock || rightBlock || basePads;
        }

        private bool ShouldKeepBazookaReinforcedGate(int row, int column)
        {
            bool sideWalls = (column <= 1 || column >= 5) && row <= 5;
            bool upperHeavy = row >= 3 && row <= 5 && column >= 1 && column <= 5;
            bool gateOpening = row <= 2 && column >= 2 && column <= 4;
            bool topCap = row == 5 && column == 3;
            return (sideWalls || upperHeavy || topCap) && !gateOpening;
        }

        private bool ShouldKeepBazookaChainReaction(int row, int column)
        {
            bool supports = row <= 2 && (column == 0 || column == 2 || column == 4 || column == 6);
            bool connectorBeam = row == 3 && column >= 0 && column <= 6;
            bool upperClusters = row >= 4 && row <= 5 && (column <= 2 || column >= 4);
            bool centralBlastGap = row == 3 && column == 3;
            return (supports || connectorBeam || upperClusters) && !centralBlastGap;
        }

        private bool ShouldKeepBazookaHeavyBunkers(int row, int column)
        {
            bool leftBunker = column <= 1 && row <= 2;
            bool centerBunker = column >= 3 && column <= 4 && row <= 3;
            bool rightBunker = column >= 5 && row <= 2;
            bool bunkerCaps = row == 3 && (column == 0 || column == 1 || column == 5 || column == 6);
            bool targetCaps = row == 4 && (column == 3 || column == 4);
            return leftBunker || centerBunker || rightBunker || bunkerCaps || targetCaps;
        }

        private bool ShouldKeepBazookaBlastCastle(int row, int column)
        {
            bool sideTowers = (column <= 1 || column >= 5) && row <= 6;
            bool centerKeep = column >= 2 && column <= 4 && row <= 5;
            bool gateGap = row <= 2 && column == 3;
            bool weakBaseNotch = row == 1 && (column == 1 || column == 5);
            bool topCrown = row == 6 && column == 3;
            return (sideTowers || centerKeep || topCrown) && !gateGap && !weakBaseNotch;
        }

        private bool ShouldKeepBazookaCollapseFortress(int row, int column)
        {
            bool lowerColumns = row <= 2 && (column == 1 || column == 3 || column == 5);
            bool upperMass = row >= 3 && row <= 6 && column >= 0 && column <= 6;
            bool upperVent = row == 4 && (column == 2 || column == 4);
            bool topNotch = row == 6 && (column == 0 || column == 6);
            return (lowerColumns || upperMass) && !upperVent && !topNotch;
        }

        private bool ShouldKeepBazookaDemolitionFortress(int row, int column)
        {
            bool leftSection = column <= 2 && row <= 5;
            bool rightSection = column >= 4 && row <= 6;
            bool centerSpine = column == 3 && row >= 1 && row <= 4;
            bool leftWeakPoint = row == 2 && column == 1;
            bool rightWeakPoint = row == 3 && column == 5;
            bool asymmetricTopCut = row == 6 && column <= 2;
            return (leftSection || rightSection || centerSpine) && !leftWeakPoint && !rightWeakPoint && !asymmetricTopCut;
        }

        private bool ShouldKeepBazookaFinalBlastFortress(int row, int column)
        {
            bool baseWall = row <= 1;
            bool sideTowers = (column <= 1 || column >= 5) && row <= 6;
            bool centralFort = column >= 2 && column <= 4 && row >= 2 && row <= 6;
            bool lowerGate = row == 2 && column == 3;
            bool midBlastWindow = row == 4 && (column == 2 || column == 4);
            bool crownOpening = row == 6 && column == 3;
            return (baseWall || sideTowers || centralFort) && !lowerGate && !midBlastWindow && !crownOpening;
        }

        private bool ShouldCreateBazookaBackLayerBrick(int row, int column)
        {
            if (!useBazookaLayout)
            {
                return false;
            }

            return bazookaLayoutPattern switch
            {
                BazookaLayoutPattern.BlastWall => IsBazookaBlastWallBackLayer(row, column),
                BazookaLayoutPattern.BlastTowers => IsBazookaBlastTowersBackLayer(row, column),
                BazookaLayoutPattern.DemolitionBlocks => IsBazookaDemolitionBlocksBackLayer(row, column),
                BazookaLayoutPattern.ReinforcedGate => IsBazookaReinforcedGateBackLayer(row, column),
                BazookaLayoutPattern.ChainReaction => IsBazookaChainReactionBackLayer(row, column),
                BazookaLayoutPattern.HeavyBunkers => IsBazookaHeavyBunkersBackLayer(row, column),
                BazookaLayoutPattern.BlastCastle => IsBazookaBlastCastleBackLayer(row, column),
                BazookaLayoutPattern.CollapseFortress => IsBazookaCollapseFortressBackLayer(row, column),
                BazookaLayoutPattern.DemolitionFortress => IsBazookaDemolitionFortressBackLayer(row, column),
                BazookaLayoutPattern.FinalBlastFortress => IsBazookaFinalBlastFortressBackLayer(row, column),
                _ => false
            };
        }

        private bool IsBazookaBlastWallBackLayer(int row, int column)
        {
            bool denseCore = row <= 4 && column >= 1 && column <= 5;
            bool reinforcedSideCaps = row >= 1 && row <= 3 && (column == 0 || column == 6);
            return denseCore || reinforcedSideCaps;
        }

        private bool IsBazookaBlastTowersBackLayer(int row, int column)
        {
            bool leftTower = column <= 2 && row <= 4;
            bool rightTower = column >= 4 && row <= 4;
            bool weakBaseCut = row == 1 && (column == 1 || column == 5);
            return (leftTower || rightTower) && !weakBaseCut;
        }

        private bool IsBazookaDemolitionBlocksBackLayer(int row, int column)
        {
            bool leftBlock = column <= 1 && row <= 3;
            bool centerBlock = column >= 3 && column <= 4 && row >= 1 && row <= 5;
            bool rightBlock = column >= 5 && row <= 4;
            return leftBlock || centerBlock || rightBlock;
        }

        private bool IsBazookaReinforcedGateBackLayer(int row, int column)
        {
            bool sideWalls = (column <= 1 || column >= 5) && row <= 5;
            bool upperHeavy = row >= 3 && row <= 5 && column >= 1 && column <= 5;
            bool gateOpening = row <= 2 && column >= 2 && column <= 4;
            return (sideWalls || upperHeavy) && !gateOpening;
        }

        private bool IsBazookaChainReactionBackLayer(int row, int column)
        {
            bool supports = row <= 2 && (column == 0 || column == 2 || column == 4 || column == 6);
            bool upperClusters = row >= 4 && row <= 5 && (column <= 2 || column >= 4);
            bool reinforcedConnectors = row == 3 && (column == 0 || column == 2 || column == 4 || column == 6);
            return supports || upperClusters || reinforcedConnectors;
        }

        private bool IsBazookaHeavyBunkersBackLayer(int row, int column)
        {
            bool leftBunker = column <= 1 && row <= 2;
            bool centerBunker = column >= 3 && column <= 4 && row <= 3;
            bool rightBunker = column >= 5 && row <= 2;
            bool bunkerCaps = row == 3 && (column == 0 || column == 1 || column == 5 || column == 6);
            return leftBunker || centerBunker || rightBunker || bunkerCaps;
        }

        private bool IsBazookaBlastCastleBackLayer(int row, int column)
        {
            bool sideTowers = (column <= 1 || column >= 5) && row <= 5;
            bool centerKeep = column >= 2 && column <= 4 && row >= 2 && row <= 5;
            bool gateGap = row <= 2 && column == 3;
            bool weakBaseNotch = row == 1 && (column == 1 || column == 5);
            return (sideTowers || centerKeep) && !gateGap && !weakBaseNotch;
        }

        private bool IsBazookaCollapseFortressBackLayer(int row, int column)
        {
            bool lowerColumns = row <= 2 && (column == 1 || column == 3 || column == 5);
            bool upperMass = row >= 3 && row <= 5 && column >= 0 && column <= 6;
            bool upperVent = row == 4 && (column == 2 || column == 4);
            return (lowerColumns || upperMass) && !upperVent;
        }

        private bool IsBazookaDemolitionFortressBackLayer(int row, int column)
        {
            bool leftSection = column <= 2 && row <= 4;
            bool rightSection = column >= 4 && row <= 5;
            bool centerSpine = column == 3 && row >= 1 && row <= 4;
            bool leftWeakPoint = row == 2 && column == 1;
            bool rightWeakPoint = row == 3 && column == 5;
            return (leftSection || rightSection || centerSpine) && !leftWeakPoint && !rightWeakPoint;
        }

        private bool IsBazookaFinalBlastFortressBackLayer(int row, int column)
        {
            bool baseWall = row <= 1;
            bool sideTowers = (column <= 1 || column >= 5) && row <= 6;
            bool centralFort = column >= 2 && column <= 4 && row >= 2 && row <= 5;
            bool lowerGate = row == 2 && column == 3;
            bool midBlastWindow = row == 4 && (column == 2 || column == 4);
            return (baseWall || sideTowers || centralFort) && !lowerGate && !midBlastWindow;
        }

        private bool ShouldSkipCannonBrick(int row, int column)
        {
            if (cannonLayoutPattern == CannonLayoutPattern.ReinforcedWall)
            {
                return row == 5 && (column == 0 || column == 6);
            }

            if (cannonLayoutPattern == CannonLayoutPattern.TwinTowersLowCenter)
            {
                bool leftTower = column <= 1;
                bool rightTower = column >= 5;
                bool centerBase = row <= 2 && column >= 2 && column <= 4;
                bool centerCap = row == 3 && column == 3;
                return !(leftTower || rightTower || centerBase || centerCap);
            }

            if (cannonLayoutPattern == CannonLayoutPattern.SteppedFortress)
            {
                int width = row switch
                {
                    0 => 7,
                    1 => 7,
                    2 => 5,
                    3 => 5,
                    4 => 3,
                    _ => 3
                };

                int sideSkip = (columns - width) / 2;
                return column < sideSkip || column >= columns - sideSkip;
            }

            if (cannonLayoutPattern == CannonLayoutPattern.HeavyGateFortress)
            {
                bool gateOpening = row <= 2 && column >= 2 && column <= 4;
                bool archGap = row == 3 && column == 3;
                bool topCornerCut = row == 5 && (column == 0 || column == 6);
                return gateOpening || archGap || topCornerCut;
            }

            if (cannonLayoutPattern == CannonLayoutPattern.TripleTowers)
            {
                bool towerColumn = column == 0 || column == 3 || column == 6;
                bool towerWidth = row <= 1 && (column == 1 || column == 2 || column == 4 || column == 5);
                bool bridgeCap = row == 2 && (column == 1 || column == 5);
                return !(towerColumn || towerWidth || bridgeCap);
            }

            if (cannonLayoutPattern == CannonLayoutPattern.AlternatingColumns)
            {
                bool reinforcedColumn = column == 0 || column == 2 || column == 4 || column == 6;
                bool lowerFill = row <= 1;
                bool bridge = row == 3 && (column == 1 || column == 3 || column == 5);
                return !(reinforcedColumn || lowerFill || bridge);
            }

            if (cannonLayoutPattern == CannonLayoutPattern.CastleKeep)
            {
                bool sideTowers = column <= 1 || column >= 5;
                bool centralKeep = column >= 2 && column <= 4 && row <= 5;
                bool gateOpening = row <= 2 && column == 3;
                bool crown = row == 6 && column >= 2 && column <= 4;
                return !(sideTowers || centralKeep || crown) || gateOpening;
            }

            if (cannonLayoutPattern == CannonLayoutPattern.TopLoadedCollapse)
            {
                bool narrowSupports = row <= 2 && (column == 1 || column == 3 || column == 5);
                bool heavyUpper = row >= 3;
                bool upperCornerCut = row == 5 && (column == 0 || column == 6);
                return !(narrowSupports || heavyUpper) || upperCornerCut;
            }

            if (cannonLayoutPattern == CannonLayoutPattern.AsymmetricFortress)
            {
                return row switch
                {
                    0 => false,
                    1 => column == 6,
                    2 => column == 4 || column == 6,
                    3 => column == 1 || column == 5 || column == 6,
                    4 => column == 4 || column == 5 || column == 6,
                    _ => column == 0 || column == 4 || column == 5 || column == 6
                };
            }

            if (cannonLayoutPattern == CannonLayoutPattern.FinalCannonFortress)
            {
                bool baseRows = row <= 1;
                bool sideBastions = (column <= 1 || column >= 5) && row <= 5;
                bool centralKeep = column >= 2 && column <= 4 && row >= 3 && row <= 5;
                bool gateOpening = row == 2 && column >= 2 && column <= 4;
                bool crown = row == 6 && column >= 2 && column <= 4;
                return !(baseRows || sideBastions || centralKeep || crown) || gateOpening;
            }

            return false;
        }

        private void CreateBrick(int row, int column, Vector3 position, string nameSuffix = "")
        {
            GameObject brick = GameObject.CreatePrimitive(PrimitiveType.Cube);
            brick.name = $"Brick_{row + 1}_{column + 1}{nameSuffix}";
            brick.transform.SetParent(transform);
            brick.transform.position = position;
            brick.transform.rotation = transform.rotation;
            brick.transform.localScale = new Vector3(brickWidth, brickHeight, brickDepth);

            Rigidbody brickRigidbody = brick.AddComponent<Rigidbody>();
            brickRigidbody.mass = brickMass;
            brickRigidbody.isKinematic = true;

            BrickPhysicsActivator activator = brick.AddComponent<BrickPhysicsActivator>();

            BoxCollider boxCollider = brick.GetComponent<BoxCollider>();
            boxCollider.size = Vector3.one;

            activator.Initialize(activationImpulse);
        }
    }
}
