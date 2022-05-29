using Godot;
using System;
using System.Collections.Generic;

public class Controls : Node
{
    public static Controls Singleton;

    static Controls()
    {
        GameManager.rand.Randomize();
    }

    public int COLORS_UNLOCKED = 2;
    public int SHAPES_UNLOCKED = 1;

    private static readonly float[] colorsUnlockTimes = new float[]
    {
        0, 0, 120, 240, 360,
    };
    private static readonly float[] shapesUnlockTimes = new float[]
    {
        0, 60, 180, 300, 420,
    };

    private float _timePassedSinceStart = 0;
    private float _timeLeft = 2 * 60 + 10;
    public (int min, int sec) TimeLeft => ((int)(_timeLeft / 60), (int)(_timeLeft % 60));
    private bool _isClockIncreasing = false;
    private float _timeToAddLeft = 0;

    [Export] public float SmoothAmount = .1f;
    [Export] public float DropDistMore = .5f;
    [Export] public float DragMinDist = 10;
    [Export] public float CardFlipAnimLength = .5f;

    [Export] private float TimePerCoin = 10;
    [Export] private float TimeAddSpeed = 10;


    [Export(PropertyHint.Range)] private float RecycleBinRange = 50;
    [Export(PropertyHint.Range)] private float ArrowRange = 100;
    [Export] public float ThrowInBinAnimLength = .5f;
    [Export] private NodePath RecycleBinPath;
    private MoveableItem _recycleBin;

    [Export] private PackedScene animPlayerPrefab;

    [Export] private PackedScene PaperClipPrefab;
    [Export] private PackedScene PaperSheetPrefab;
    [Export] private PackedScene ContractPrefab;

    [Export] private PackedScene StampColorPaletPrefab;
    [Export] private PackedScene StampPrefab;

    [Export] private NodePath newContractProgressBarPath;
    private NewContractProgressBar _newContractProgressBar;

    private GameManager _gameManager;

    [Export] private NodePath emptyPaperSheetGenerator;
    private PaperSheet _emptyPaperGen;
    private Vector2 _emptyPaperGenPos;

    [Export] private NodePath paperClipGenerator;
    private PaperClip _paperClipGen;
    private Vector2 _paperClipGenPos;

    [Export] private PackedScene GroupedPapersPrefab;
    [Export] private PackedScene CoinPrefab;

    [Export] private NodePath arrowPath;
    private Arrow _arrow;

    private Vector2 _mouseDownPos;
    private Vector2 _mousePixPos;
    private MoveableItem _movedItem = null;
    /// <summary> Position where the item was grabbed relative to the item left up corner </summary>
    private Vector2 _grabPos;
    private bool _isDragging = false;
    private bool IsMoving => _movedItem != null;

    private bool _isDrawingWithPencil = false;

    private Contract _acceptedContract;

    private int _noSpawnedContracts = 0;
    public int NoSpawnedContracts => _noSpawnedContracts;

    private bool isGameEnded = false;

    private GameStats stats = new GameStats();

    public void SetGameManager(GameManager gameManager)
        => _gameManager = gameManager;

    public override void _Ready()
    {
        Singleton = this;
        _recycleBin = GetNode<MoveableItem>(RecycleBinPath);

        _emptyPaperGen = GetNode<PaperSheet>(emptyPaperSheetGenerator);
        _emptyPaperGenPos = _emptyPaperGen.Position;
        _paperClipGen = GetNode<PaperClip>(paperClipGenerator);
        _paperClipGenPos = _paperClipGen.Position;

        _newContractProgressBar = GetNode<NewContractProgressBar>(newContractProgressBarPath);

        _arrow = GetNode<Arrow>(arrowPath);
    }

    private void SpawnNewContract()
    {
        int noPages = GameManager.rand.RandiRange((int)(_timePassedSinceStart / 60) + 2, (int)(_timePassedSinceStart / 60) + 4);
        List<(StampShape, StampColor)> shapes = new List<(StampShape, StampColor)>();
        for (int i = 0; i < noPages; i++)
        {
            StampShape shape = (StampShape)(int)(GameManager.rand.Randi() % (uint)(SHAPES_UNLOCKED));
            StampColor color = (StampColor)(int)(GameManager.rand.Randi() % (uint)(COLORS_UNLOCKED));

            shapes.Add((shape, color));
        }

        shapes.Sort((t1, t2) =>
        {
            int a = ((int)t1.Item1).CompareTo((int)t2.Item1);
            if (a != 0) return a;
            return ((int)t1.Item2).CompareTo((int)t2.Item2);
        });

        int coins = (int)(Mathf.Sqrt(noPages));

        Contract contract = SpawnNewMoveableItem<Contract>(ContractPrefab);

        contract.Constructor(coins, shapes);
        contract.MoveInstantly(new Vector2(-contract.GetSize.x / 2, GetViewport().Size.y - contract.GetSize.y));
        contract.Move(new Vector2(contract.GetSize.x, GetViewport().Size.y - contract.GetSize.y));
        _noSpawnedContracts++;
    }

    public override void _Process(float delta)
    {
        if (isGameEnded) return;

        if (_isClockIncreasing)
        {
            float toAdd = Mathf.Min(delta * TimeAddSpeed, _timeToAddLeft);

            _timeLeft += toAdd;
            _timeToAddLeft -= toAdd;

            if (_timeToAddLeft <= .01f) _isClockIncreasing = false;
        }
        else
        {
            _timePassedSinceStart += delta;
            _timeLeft -= delta;
            if (_timeLeft <= 0)
            {
                _timeLeft = 0;

                stats.timePlayed = _timePassedSinceStart;
                _gameManager.EndGame(stats);
                isGameEnded = true;

                for (int i = 0; i < GetChildCount(); i++)
                {
                    GetChild<MoveableItem>(i).PlayGameOverAnim();
                }

                return;
            }

            if (COLORS_UNLOCKED < colorsUnlockTimes.Length)
                if (colorsUnlockTimes[COLORS_UNLOCKED - 1 + 1] <= _timePassedSinceStart)
                {
                    COLORS_UNLOCKED++;

                    StampColorPalet palet = SpawnNewMoveableItem<StampColorPalet>(StampColorPaletPrefab);
                    palet.Constructor((StampColor)(COLORS_UNLOCKED - 1));
                    palet.MoveInstantly(new Vector2(-palet.GetSize.x / 2, palet.GetSize.y));
                    palet.Move(new Vector2(palet.GetSize.x, palet.GetSize.y), false, true);
                }
            if (SHAPES_UNLOCKED < shapesUnlockTimes.Length)
                if (shapesUnlockTimes[SHAPES_UNLOCKED - 1 + 1] <= _timePassedSinceStart)
                {
                    SHAPES_UNLOCKED++;

                    Stamp stamp = SpawnNewMoveableItem<Stamp>(StampPrefab);
                    stamp.Constructor((StampShape)(SHAPES_UNLOCKED - 1));
                    stamp.MoveInstantly(new Vector2(-stamp.GetSize.x / 2, stamp.GetSize.y));
                    stamp.Move(new Vector2(stamp.GetSize.x, stamp.GetSize.y), false, true);
                }
        }

        if (_noSpawnedContracts < 3)
        {
            if (_newContractProgressBar.IsContractLoaded)
            {
                _newContractProgressBar.TakeLoadedContract();
                SpawnNewContract();
                GD.Print("Spawned contract, ", _noSpawnedContracts);
            }
        }

        if (_isDrawingWithPencil)
        {
            if (_movedItem is Pencil pencil)
            {
                Vector2 tip = pencil.GetPenTipPos();
                MoveableItem item = GetItemInPosition<MoveableItem>(tip, pencil);
                if (item is PaperSheet paper)
                {
                    paper.Draw(tip, paper.Position);
                }
                else if (item is GroupedPapers group)
                {
                    group.GetFirstPaper().Draw(tip, group.Position);
                }
            }
            else if (_movedItem is Eraser eraser)
            {
                Vector2 tip = eraser.GetTipPos();
                MoveableItem item = GetItemInPosition<MoveableItem>(tip, eraser);
                if (item is PaperSheet paper)
                {
                    paper.Erase(tip, paper.Position);
                }
                else if (item is GroupedPapers group)
                {
                    group.GetFirstPaper().Erase(tip, group.Position);
                }
            }
            else throw new Exception();
        }

        if (IsMoving)
        {
            MoveChild(_movedItem, GetChildCount() - 1);
        }
    }

    private T SpawnNewMoveableItem<T>(PackedScene prefab) where T : MoveableItem
    {
        T item = prefab.Instance<T>();
        AddChild(item);
        MoveChild(item, GetChildCount() - 1);
        return item;
    }

    private T GetItemInPosition<T>(Vector2 pos, MoveableItem expect = null) where T : MoveableItem
    {
        for (int i = GetChildCount() - 1; i >= 0; i--)
        {
            var itemO = GetChild(i);
            if (!(itemO is MoveableItem)) continue;
            if ((expect != null) && ReferenceEquals(expect, itemO)) continue;

            MoveableItem item = (MoveableItem)itemO;

            if (item.IsDestroyed) continue;

            if (item.IsInside(pos))
            {
                if (typeof(T).IsAssignableFrom(item.GetType())) return (T)item;
                return null;
            }
        }
        return null;
    }

    public override void _Input(InputEvent e)
    {
        if (isGameEnded) return;

        if (e is InputEventMouseMotion eMouseMove)
        {
            _mousePixPos = eMouseMove.Position;

            if (IsMoving)
            {
                if (!_isDragging && _mousePixPos.DistanceTo(_mouseDownPos) > DragMinDist)
                    _isDragging = true;

                if (_isDragging)
                {
                    MoveItem();
                }
            }
        }
        else if (e is InputEventMouseButton eMouseBut)
        {
            _mousePixPos = eMouseBut.Position;

            if (eMouseBut.ButtonIndex == (int)ButtonList.Left)
            {
                if (eMouseBut.Pressed)
                {
                    _mouseDownPos = eMouseBut.Position;

                    MoveableItem item = GetItemInPosition<MoveableItem>(_mousePixPos);
                    if (item != null)
                    {
                        PickUpItem(item);
                    }
                }
                else  // released
                {
                    _isDrawingWithPencil = false;

                    if (_isDragging)
                        _isDragging = false;
                    else
                    {
                        // didnt drag -> pressed
                        if (_movedItem != null)
                            _movedItem.Pressed();
                    }

                    if (IsMoving)
                        DropItem();
                }
            }
            else if (eMouseBut.ButtonIndex == (int)ButtonList.Right)
            {
                if (IsMoving)
                {
                    if ((_movedItem is Pencil) || (_movedItem is Eraser))
                    {
                        if (eMouseBut.Pressed)
                        {
                            _isDrawingWithPencil = true;
                        }
                        else if (!eMouseBut.Pressed)
                        {
                            if (_isDrawingWithPencil)
                            {
                                _isDrawingWithPencil = false;
                            }
                        }
                    }
                    else if (_movedItem is PaperClip clip)
                    {
                        PaperSheet item = GetItemInPosition<PaperSheet>(clip.Position, clip);
                        if (item != null)
                        {
                            if (!ReferenceEquals(item, _emptyPaperGen))
                            {
                                GroupedPapers group = GroupedPapersPrefab.Instance<GroupedPapers>();

                                group.Constructor(clip, item);

                                AddChild(group);
                                MoveChild(group, GetChildCount() - 1);

                                DropItem();
                                stats.paperclipsUsed++;
                            }
                        }
                    }
                    else if (_movedItem is PaperSheet paper)
                    {
                        GroupedPapers group = GetItemInPosition<GroupedPapers>(paper.Position, paper);
                        if (group != null)
                        {
                            group.AddPaper(paper);

                            MoveChild(group, 0);
                            MoveChild(group, GetChildCount() - 1);

                            DropItem();
                        }
                    }
                    else if (_movedItem is Coin coin)
                    {
                        AlarmClock clock = GetItemInPosition<AlarmClock>(coin.Position, coin);
                        if (clock != null)
                        {
                            clock.InsertCoin(coin);
                            _isClockIncreasing = true;
                            _timeToAddLeft += TimePerCoin;
                            _movedItem = null;
                        }
                    }
                    else if (_movedItem is GroupedPapers group)
                    {
                        Contract contract = GetItemInPosition<Contract>(group.Position, group);
                        if ((contract != null) && contract.Sealed)
                        {
                            if (group.IsContractMet(contract))
                            {
                                contract.PlayCoinsAudio();

                                // spawn coins

                                for (int i = 0; i < contract.CoinsValue; i++)
                                {
                                    Vector2 pos = contract.Position + new Vector2(
                                        GameManager.rand.RandfRange(-contract.GetSize.x / 2, contract.GetSize.x / 2),
                                        GameManager.rand.RandfRange(-contract.GetSize.y / 2, contract.GetSize.y / 2));

                                    Coin coinIns = CoinPrefab.Instance<Coin>();
                                    AddChild(coinIns);
                                    coinIns.MoveInstantly(pos);
                                }

                                _movedItem = null;

                                _acceptedContract = null;

                                Vector2 targetPos = _arrow.Position + new Vector2(128 + contract.GetSize.x, 0);

                                contract.Move(targetPos, true, true);
                                MoveChild(contract, GetChildCount() - 1);
                                group.Move(targetPos, true, true);
                                MoveChild(group, GetChildCount() - 1);
                                _noSpawnedContracts--;
                                stats.contractsFinished++;
                            }
                            else
                            {
                                if (eMouseBut.Pressed)
                                    contract.PlayErrorAudio();
                                GD.Print("NOT MET");
                            }
                        }
                    }
                    else
                    {
                        if (eMouseBut.Pressed)
                        {
                            _movedItem.RightClicked();
                        }
                    }
                }
            }
        }
    }

    private void PickUpItem(MoveableItem item)
    {
        if (IsMoving) throw new Exception("Already moving an item");

        _movedItem = item;
        MoveChild(item, GetChildCount() - 1);
        _movedItem.IsBeingMoved = true;
        _grabPos = _mousePixPos - _movedItem.Position;

        if (ReferenceEquals(item, _emptyPaperGen))
        {
            PaperSheet paper = PaperSheetPrefab.Instance<PaperSheet>();
            paper.Position = _emptyPaperGenPos;

            AddChild(paper);
            _emptyPaperGen = paper;
        }
        if (ReferenceEquals(item, _paperClipGen))
        {
            _paperClipGen = SpawnNewMoveableItem<PaperClip>(PaperClipPrefab);
            _paperClipGen.MoveInstantly(_paperClipGenPos);
        }
        if (item is Contract contract)
        {
            if (contract.IsSigned && !contract.Sealed && (_acceptedContract == null))
            {
                _arrow.Shown = true;
            }
        }
    }
    private void MoveItem()
    {
        if (!IsMoving) throw new Exception("Not moving");

        _movedItem.Move(_mousePixPos - _grabPos);
    }
    private void DropItem()
    {
        if (!IsMoving) throw new Exception("Not moving");


        if (_movedItem is Contract contract)
        {
            _arrow.Shown = false;

            if (contract.IsSigned && !contract.Sealed)
            {
                if ((contract.GetStampColor == StampColor.Green) && (_acceptedContract != null))
                {
                    // another contract is already accepted
                }
                else
                {
                    if (_arrow.Position.DistanceTo(contract.Position) < ArrowRange)
                    {
                        contract.PlayDisappearAnim(_arrow.Position + new Vector2(128 + contract.GetSize.x, 0));
                        if (contract.GetStampColor == StampColor.Green)
                        {
                            _acceptedContract = contract;
                            _arrow.StartCircleAnim(ShowSealedContract);
                        }
                        else
                        {
                            _noSpawnedContracts--;
                            stats.contractsDeclined++;
                        }
                    }
                }
            }
        }

        // is near the recycle bin
        else if (!ReferenceEquals(_movedItem, _recycleBin))
        {
            if (_recycleBin.Position.DistanceTo(_movedItem.Position) <= RecycleBinRange)
            {
                if (!_movedItem.CanBeThrownAway)
                {
                    Node2D box2 = _movedItem.Duplicate(11) as Node2D;
                    AddChild(box2);

                    ThrowAwayAnimPlayer animPlayer = animPlayerPrefab.Instance<ThrowAwayAnimPlayer>();
                    box2.AddChild(animPlayer);
                    animPlayer.SetMainNode(box2);
                    animPlayer.PlayDestroyAnimation(_movedItem.TargetPos, _recycleBin.Position + new Vector2(0, -20));

                    float randHeight = GameManager.rand.RandfRange(_movedItem.GetSize.y, GetViewport().Size.y - _movedItem.GetSize.y);
                    _movedItem.MoveInstantly(new Vector2(-_movedItem.GetSize.x / 2, randHeight));
                    _movedItem.Move(new Vector2(_movedItem.GetSize.x, randHeight));
                }
                else
                {
                    // throw away
                    Vector2 targetPos = _movedItem.TargetPos;
                    _movedItem.SetDestroyed();

                    ThrowAwayAnimPlayer animPlayer = animPlayerPrefab.Instance<ThrowAwayAnimPlayer>();
                    _movedItem.AddChild(animPlayer);
                    animPlayer.SetMainNode(_movedItem);
                    animPlayer.PlayDestroyAnimation(targetPos, _recycleBin.Position + new Vector2(0, -20));
                }
            }
        }

        _isDrawingWithPencil = false;
        _movedItem.IsBeingMoved = false;
        _movedItem = null;
    }

    public void StampPrinted(Stamp stamp, Vector2 pos)
    {
        MoveableItem item = GetItemInPosition<MoveableItem>(pos);

        if (item == null) return;

        if (stamp.HasInk)
        {
            if (item is Contract contract)
            {
                if (stamp.IsContractStamp && !contract.Sealed)
                {
                    contract.Stamp(pos, stamp.GetStampColor);
                    stamp.RemoveInk();
                    stats.stamps++;
                }
            }
            else if (item is PaperSheet paper)
            {
                if (!stamp.IsContractStamp)
                {
                    paper.Stamp(pos, paper.Position, stamp.GetStampShape, stamp.GetStampColor);
                    stamp.RemoveInk();
                    stats.stamps++;
                }
            }
            else if (item is GroupedPapers group)
            {
                if (!stamp.IsContractStamp)
                {
                    group.GetFirstPaper().Stamp(pos, group.Position, stamp.GetStampShape, stamp.GetStampColor);
                    stamp.RemoveInk();
                    stats.stamps++;
                }
            }
        }
        else
        {
            if (item is StampColorPalet colorPalet)
            {
                stamp.SetInk(colorPalet.GetStampColor);
            }
        }
    }

    public void ContractDisappeared(Contract contract)
    {
        if (contract.GetStampColor == StampColor.Red)
        {
            contract.QueueFree();
        }
    }

    private void ShowSealedContract()
    {
        _acceptedContract.Seal();
        _acceptedContract.MoveInstantly(_arrow.Position + new Vector2(128 + _acceptedContract.GetSize.x, 0));
        _acceptedContract.Move(GetViewport().Size - (_acceptedContract.GetSize / 2) - new Vector2(_acceptedContract.GetSize.x / 2, _acceptedContract.GetSize.x / 2));
    }
}