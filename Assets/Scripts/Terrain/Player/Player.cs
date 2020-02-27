using Terrain.Items;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Terrain.Player
{
    [RequireComponent(typeof(ItemManager))]
    [RequireComponent(typeof(PlayerMovement))]
    [RequireComponent(typeof(PlayerArmManager))]
    [RequireComponent(typeof(PlayerInput))]
    public class Player : MonoBehaviour
    {
        public static Player Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType<Player>();
                return _instance;
            }
        }
        private static Player _instance;
        /// <summary>
        /// Shorthand for <see cref="Player.Instance.PlayerInput"/>.
        /// </summary>
        public static PlayerInput Input { get { return Instance.PlayerInput; } }

        public ItemManager ItemManager
        {
            get
            {
                if (_items == null)
                    _items = GetComponent<ItemManager>();
                return _items;
            }
        }
        private ItemManager _items;
        public PlayerMovement Movement
        {
            get
            {
                if (_move == null)
                    _move = GetComponent<PlayerMovement>();
                return _move;
            }
        }
        private PlayerMovement _move;
        public PlayerArmManager ArmManager
        {
            get
            {
                if (_arms == null)
                    _arms = GetComponent<PlayerArmManager>();
                return _arms;
            }
        }
        private PlayerArmManager _arms;
        public PlayerInput PlayerInput
        {
            get
            {
                if (_input == null)
                    _input = GetComponent<PlayerInput>();
                return _input;
            }
        }
        private PlayerInput _input;

        public string Name = "Bob";

        private void Awake()
        {
            _instance = this;
        }
    }
}