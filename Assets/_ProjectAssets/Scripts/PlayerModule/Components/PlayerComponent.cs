using Anura.ConfigurationModule.Managers;
using Photon.Pun;
using UnityEngine;

public class PlayerComponent : MonoBehaviour
{
    [SerializeField]
    private BasePlayerComponent basePlayerComponent;
    [SerializeField]
    private PlayerGraphicsBehaviour playerGraphicsBehaviour;

    private GameInputActions.PlayerActions playerActions;
    private PlayerMotionBehaviour playerMotionBehaviour;
    private bool isMultiplayer;
    private PhotonView photonView;
    [field: SerializeField] public Transform EmojiHolder { get; private set; }


    private void Awake()
    {
        isMultiplayer = ConfigurationManager.Instance.Config.GetIsMultiplayer();

        photonView = GetComponent<PhotonView>();
        playerMotionBehaviour = GetComponent<PlayerMotionBehaviour>();
    }
    private void Start()
    {
        isMultiplayer = ConfigurationManager.Instance.Config.GetIsMultiplayer();
        photonView = GetComponent<PhotonView>();

        if (!isMultiplayer)
        {
            photonView.enabled = false;
            GetComponent<PhotonTransformView>().enabled = false;
            photonView = null;
            BotManager.Instance.RegisterBotEnemy(this);
        }

        if (photonView != null && !photonView.IsMine) {
            SetupOtherPlayer();
            return;
        }

        SetupMyPlayer();
    }

    private void OnEnable()
    {
        if (!isMultiplayer || (photonView != null && photonView.IsMine))
        {
            RoomStateManager.OnStateUpdated += OnStateUpdatedForMyPlayer;
            InputManager.OnDragged += MobileMovement;
            InputManager.OnDragEnded += StopMobileMovement;
            InputManager.OnDoubleTap += MobileJump;
        }
    }

    private void OnDisable()
    {
        photonView = GetComponent<PhotonView>();

        if (photonView != null && photonView.IsMine)
        {
            RoomStateManager.OnStateUpdated -= OnStateUpdatedForMyPlayer;
            InputManager.OnDragged -= MobileMovement;
            InputManager.OnDragEnded -= StopMobileMovement;
            InputManager.OnDoubleTap -= MobileJump;
        }
    }

    private void MobileMovement(Vector3 _direction)
    {
        playerMotionBehaviour.SetMovementDirection(_direction.x);
    }
    
    private void StopMobileMovement()
    {
        playerMotionBehaviour.SetMovementDirection(0);
    }
    
    private void MobileJump(Vector2 _obj)
    {
        playerMotionBehaviour.OnJumpPerformed();
    }

    private void SetupMyPlayer()
    {
        PlayerManager.Instance.RegisterMyPlayer(this);
        basePlayerComponent.PreSetup();

        playerActions = GameInputManager.Instance.GetPlayerActionMap().GetPlayerActions();

        playerMotionBehaviour.RegisterMovementCallbacks(playerActions);
        playerMotionBehaviour.RegisterJumpCallbacks(playerActions);
        playerMotionBehaviour.RegisterPlayerState(basePlayerComponent.state);

        playerGraphicsBehaviour.RegisterPlayerState(basePlayerComponent.state);
        PlayerManager.Instance.onHealthUpdated += playerGraphicsBehaviour.OnHealthUpdated;

        var playerIndicatorBehaviour = GetComponentInChildren<PlayerIndicatorBehaviour>();
        playerIndicatorBehaviour.RegisterDirectionCallbacks(playerActions);

        var playerThrowBehaviour = GetComponentInChildren<PlayerThrowBehaviour>();
        playerThrowBehaviour.RegisterThrowCallbacks(playerActions);

        basePlayerComponent.PostSetup();
        playerActions.Disable();
    }

    private void SetupOtherPlayer()
    {
        PlayerManager.Instance.otherPlayerTransform = transform;
        PlayerManager.Instance.OtherPlayerComponent = this;
        Destroy(GetComponent<Rigidbody2D>());
    }

    private void OnStateUpdatedForMyPlayer(IRoomState roomState)
    {
        basePlayerComponent.state.SetHasWeaponOut(-1);
        if (roomState is MyTurnState)
        {
           playerActions.Enable(); 
        }
        else
        {
            playerActions.Disable();
        }
    }
}
