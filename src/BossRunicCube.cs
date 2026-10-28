#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Intents;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace AnimeSurvivalMod
{
    /// <summary>
    /// Босс Акта 2: Рунический Куб (The Runic Cube / The Architect's Puzzle).
    /// Мистический левитирующий артефакт древних зодчих Шпиля.
    /// Переключает рунические грани 3x3 под свои намерения:
    /// - Бастион Рун (Defend / Лазурный Щит)
    /// - Матрица Клинков (Attack / Рубиновые Мечи)
    /// - Гексагон Казни (Skull / Токсичный Череп)
    /// </summary>
    public class RunicCubeBoss : MonsterModel
    {
        public override int MinInitialHp => 340;
        public override int MaxInitialHp => 360;

        protected override string VisualsPath => SceneHelper.GetScenePath("creature_visuals/the_forgotten");
        public override LocString Title => new LocString("monsters", "RUNIC_CUBE.name");

        public override DamageSfxType TakeDamageSfxType => DamageSfxType.Stone;
        protected override string AttackSfx => "event:/sfx/enemy/enemy_attacks/the_forgotten/the_forgotten_attack";
        protected override string CastSfx => "event:/sfx/enemy/enemy_attacks/the_forgotten/the_forgotten_cast";
        public override string DeathSfx => "event:/sfx/enemy/enemy_attacks/the_forgotten/the_forgotten_die";

        public string CurrentVisualPattern { get; private set; } = "defend";
        private RunicCubeVisualsController? _controller;

        public void RegisterVisualsController(RunicCubeVisualsController controller)
        {
            _controller = controller;
            _controller.PlayPattern(CurrentVisualPattern);
        }

        public void SetVisualPattern(string pattern)
        {
            if (CurrentVisualPattern == pattern && _controller != null && !_controller.IsPlaying) return;
            CurrentVisualPattern = pattern;
            _controller?.PlayPattern(pattern);
        }

        public void TriggerHit()
        {
            _controller?.PlayTemporaryPattern("hit", 0.4f);
        }

        public override async Task AfterAddedToRoom()
        {
            await base.AfterAddedToRoom();
            // Начальная грань: собирается защитный рунический щит
            SetVisualPattern("defend");
            // Древний артефакт защищен двумя зарядами Артефакта
            await PowerCmd.Apply<ArtifactPower>(new ThrowingPlayerChoiceContext(), Creature, 2m, Creature, null);
        }

        protected override MonsterMoveStateMachine GenerateMoveStateMachine()
        {
            var stateBastion = new MoveState("RUNIC_BASTION", MoveRunicBastion, new DefendIntent(), new BuffIntent());
            var stateBlades = new MoveState("BLADE_MATRIX", MoveBladeMatrix, new MultiAttackIntent(6, 3));
            var stateExecution = new MoveState("HEX_EXECUTION", MoveHexExecution, new SingleAttackIntent(32), new DebuffIntent());

            stateBastion.FollowUpState = stateBlades;
            stateBlades.FollowUpState = stateExecution;
            stateExecution.FollowUpState = stateBastion;

            return new MonsterMoveStateMachine(new MonsterState[] { stateBastion, stateBlades, stateExecution }, stateBastion);
        }

        /// <summary>
        /// Ход 1: Бастион Рун (щит уже собран и зафиксирован на лицевой грани).
        /// Даёт блок и шипы, а в конце хода плавно собирает Мечи для следующего хода.
        /// </summary>
        private async Task MoveRunicBastion(IReadOnlyList<Creature> targets)
        {
            await CreatureCmd.GainBlock(Creature, 25, ValueProp.Move, null);
            await PowerCmd.Apply<ThornsPower>(new ThrowingPlayerChoiceContext(), Creature, 3m, Creature, null);
            await Cmd.Wait(0.2f);

            // Поворачиваем грани, собирая Матрицу Клинков на следующий ход
            SetVisualPattern("attack");
        }

        /// <summary>
        /// Ход 2: Матрица Клинков (мечи уже собраны).
        /// Атакует 3 раза, а в конце хода собирает Череп для супер-удара.
        /// </summary>
        private async Task MoveBladeMatrix(IReadOnlyList<Creature> targets)
        {
            await ModVfx.AttackLunge(Creature, -50f, 0.12f);

            for (int i = 0; i < 3; i++)
            {
                await DamageCmd.Attack(6).FromMonster(this)
                    .WithAttackerAnim("Attack", 0.12f)
                    .OnlyPlayAnimOnce()
                    .WithAttackerFx(null, AttackSfx)
                    .WithHitFx("vfx/vfx_attack_slash")
                    .Execute(null);
                await Cmd.Wait(0.08f);
            }
            await Cmd.Wait(0.2f);

            // Поворачиваем грани, собирая Гексагон Казни (Череп)
            SetVisualPattern("skull");
        }

        /// <summary>
        /// Ход 3: Гексагон Казни (череп уже собран).
        /// Наносит 32 урона + дебаффы, а в конце возвращает грань Щита.
        /// </summary>
        private async Task MoveHexExecution(IReadOnlyList<Creature> targets)
        {
            await ModVfx.AttackLunge(Creature, -80f, 0.18f);

            await DamageCmd.Attack(32).FromMonster(this)
                .WithAttackerAnim("HeavyAttack", 0.3f)
                .OnlyPlayAnimOnce()
                .WithAttackerFx(null, AttackSfx)
                .WithHitFx("vfx/vfx_attack_blunt")
                .Execute(null);

            var choice = new ThrowingPlayerChoiceContext();
            foreach (Creature target in targets)
            {
                await PowerCmd.Apply<VulnerablePower>(choice, target, 2m, Creature, null);
                await PowerCmd.Apply<WeakPower>(choice, target, 2m, Creature, null);
            }
            await Cmd.Wait(0.2f);

            // Возврат к Бастиону Рун для нового цикла
            SetVisualPattern("defend");
        }

        public override CreatureAnimator GenerateAnimator(MegaSprite controller)
        {
            if (controller == null) return null!;
            try
            {
                return ModelDb.Monster<MegaCrit.Sts2.Core.Models.Monsters.SpectralKnight>().GenerateAnimator(controller);
            }
            catch
            {
                AnimState animState = new AnimState("idle_loop", isLooping: true);
                AnimState state = new AnimState("die");
                CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
                creatureAnimator.AddAnyState("Idle", animState);
                creatureAnimator.AddAnyState("Dead", state);
                return creatureAnimator;
            }
        }
    }

    /// <summary>
    /// Босс-энкаунтер Рунического Куба для Акта 2.
    /// </summary>
    public class RunicCubeBossEncounter : EncounterModel
    {
        public override RoomType RoomType => RoomType.Boss;
        public override string BossNodePath => "res://images/map/placeholder/soul_fysh_boss_icon";
        public override MegaSkeletonDataResource? BossNodeSpineResource => null;
        public override IEnumerable<MonsterModel> AllPossibleMonsters => new[] { ModelDb.Monster<RunicCubeBoss>() };
        public override float GetCameraScaling() => 0.85f;
        public override Vector2 GetCameraOffset() => Vector2.Down * 60f;

        protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
        {
            return new List<(MonsterModel, string?)>
            {
                (ModelDb.Monster<RunicCubeBoss>().ToMutable(), null)
            };
        }
    }

    /// <summary>
    /// Контроллер спрайта кубика: воспроизводит 16-кадровый поворот слоев один раз,
    /// после чего останавливается на 15-м кадре (собранном узоре), фиксируя грань до следующего хода.
    /// </summary>
    public class RunicCubeVisualsController
    {
        private readonly Sprite2D _sprite;
        private readonly string _assetDir;
        private readonly Dictionary<string, Texture2D> _textureCache = new();
        private string _activePattern = "defend";
        private int _currentFrame = 0;
        private bool _isPlaying = false;
        private string? _tempPattern;
        private ulong _tempPatternEndTime;

        public bool IsPlaying => _isPlaying;

        public RunicCubeVisualsController(Sprite2D sprite, string assetDir)
        {
            _sprite = sprite;
            _assetDir = assetDir;
        }

        public void PlayPattern(string pattern)
        {
            _activePattern = pattern;
            _tempPattern = null;
            _currentFrame = 0;
            _isPlaying = true;
            ApplyPattern(pattern, frame: 0);
        }

        public void PlayTemporaryPattern(string pattern, float durationSec)
        {
            _tempPattern = pattern;
            _tempPatternEndTime = Time.GetTicksMsec() + (ulong)(durationSec * 1000f);
            _currentFrame = 0;
            _isPlaying = true;
            ApplyPattern(pattern, frame: 0);
        }

        public void OnFrameTick()
        {
            if (!GodotObject.IsInstanceValid(_sprite) || _sprite.Texture == null) return;

            // Если временный эффект (Hit) закончился - возвращаемся к активному узору
            if (_tempPattern != null && Time.GetTicksMsec() >= _tempPatternEndTime)
            {
                _tempPattern = null;
                ApplyPattern(_activePattern, frame: 15);
                _isPlaying = false;
                return;
            }

            // Плавное пошаговое проигрывание поворотов
            if (_isPlaying && _sprite.Hframes > 1)
            {
                int totalFrames = _sprite.Hframes * _sprite.Vframes; // 16 кадров
                if (_currentFrame < totalFrames - 1)
                {
                    _currentFrame++;
                    _sprite.Frame = _currentFrame;
                }
                else
                {
                    // Достигли финального 15-го кадра (узор 3x3 полностью собран)!
                    // ОСТАНАВЛИВАЕМ вращение и оставляем собранный узор до следующего действия:
                    _isPlaying = false;
                    _sprite.Frame = totalFrames - 1;
                }
            }
        }

        private void ApplyPattern(string pattern, int frame)
        {
            if (!GodotObject.IsInstanceValid(_sprite)) return;

            // 1. Сначала ищем 16-кадровый спрайтшит 4x4
            string sheetPath = Path.Combine(_assetDir, $"{pattern}_sheet.png");
            var tex = GetOrLoadTexture(sheetPath);
            if (tex != null)
            {
                _sprite.Texture = tex;
                _sprite.Hframes = 4;
                _sprite.Vframes = 4;
                _sprite.Frame = Mathf.Clamp(frame, 0, 15);
                return;
            }

            // 2. Fallback: одиночный PNG
            string singlePath = Path.Combine(_assetDir, $"{pattern}.png");
            tex = GetOrLoadTexture(singlePath);
            if (tex != null)
            {
                _sprite.Texture = tex;
                _sprite.Hframes = 1;
                _sprite.Vframes = 1;
                _sprite.Frame = 0;
                _isPlaying = false;
            }
        }

        private Texture2D? GetOrLoadTexture(string fullPath)
        {
            if (_textureCache.TryGetValue(fullPath, out var cached)) return cached;
            if (!File.Exists(fullPath))
            {
                Log.Warn($"[AnimeSurvivalMod] RunicCube texture not found: {fullPath}");
                return null;
            }

            try
            {
                var img = Image.LoadFromFile(fullPath);
                if (img != null)
                {
                    var tex = ImageTexture.CreateFromImage(img);
                    _textureCache[fullPath] = tex;
                    Log.Info($"[AnimeSurvivalMod] Loaded RunicCube texture: {Path.GetFileName(fullPath)} ({img.GetWidth()}x{img.GetHeight()})");
                    return tex;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[AnimeSurvivalMod] Error loading texture {fullPath}: {ex}");
            }
            return null;
        }
    }
}
