using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using AChen.Networking;
using AChen.Player;
using Cysharp.Threading.Tasks;

//抽卡界面的卡牌列表控制器
public class CardPickController : MonoBehaviour {
    [SerializeField] private string _poolKey = "Card01";
    [SerializeField] private int _drawCount = 1;
    [SerializeField] private GameObjectHorizontalLayout _gameObjectHorizontalLayout;
    [SerializeField] private GameObject _cardPrefab;
    [SerializeField] private Button _drawButton;
    [SerializeField] private Button _checkNextCardButton;
    private readonly List<CardPickView> _cardObjects = new List<CardPickView>();
    private int _currentCardIndex = 0;
    private bool _drawing;
    private bool _drawn;

    void Awake()
    {
        if (_gameObjectHorizontalLayout != null)
        {
            _gameObjectHorizontalLayout.enabled = false;
        }

        if (_drawButton == null)
        {
            _drawButton = _checkNextCardButton;
        }

        if (_drawButton != null)
        {
            _drawButton.onClick.AddListener(OnPrimaryClick);
        }

        if (_checkNextCardButton != null && _checkNextCardButton != _drawButton)
        {
            _checkNextCardButton.gameObject.SetActive(false);
            _checkNextCardButton.onClick.AddListener(CheckNextCard);
        }
    }

    void OnPrimaryClick()
    {
        if (!_drawn)
        {
            SubmitDraw().Forget();
            return;
        }

        CheckNextCard();
    }

    async UniTaskVoid SubmitDraw()
    {
        if (_drawing)
        {
            return;
        }

        _drawing = true;
        if (_drawButton != null)
        {
            _drawButton.interactable = false;
        }

        ALog.Log($"提交抽卡. Pool={_poolKey}; Count={_drawCount}", ALogCategories.Net);
        try
        {
            if (!PlayerSession.HasInstance || !PlayerSession.Instance.IsAuthenticated)
            {
                throw new BackendApiException(401, "INVALID_ACCESS_TOKEN", "登录状态已失效，请重新登录");
            }

            CardDrawResponse response = await PlayerSession.Instance.DrawCardsAsync(_poolKey, _drawCount);
            ALog.Log($"抽卡成功. Pool={_poolKey}; Count={response.Results.Count}; Revision={response.Player.Revision}", ALogCategories.Net);
            _drawn = true;
            await BuildCardsAsync(response.Results);
        }
        catch (BackendApiException exception)
        {
            ALog.LogWarning($"抽卡失败. Pool={_poolKey}; Count={_drawCount}; Code={exception.Code}; Status={exception.StatusCode}", ALogCategories.Net);
            if (_drawButton != null)
            {
                _drawButton.interactable = true;
            }
        }
        finally
        {
            _drawing = false;
        }
    }

    async UniTask BuildCardsAsync(IReadOnlyList<CardDrawResult> results)
    {
        for (int i = 0; i < results.Count; i++)
        {
            CardDrawResult result = results[i];
            GameObject card = Instantiate(_cardPrefab, transform);
            CardPickView view = card.GetComponent<CardPickView>();
            string sourcePool = string.IsNullOrEmpty(result.SourcePool) ? _poolKey : result.SourcePool;
            Texture texture = await LoadCardTextureAsync(sourcePool, result.CardId);
            view.Init(new CardPickViewData
            {
                cardId = result.CardId,
                cardShaderType = ToShaderType(result.Rarity),
                cardTexture = texture
            });
            view.gameObject.SetActive(false);
            view.SwitchStatus(CardStatus.None);
            view.gameObject.transform.localPosition = new Vector3(0, 0, i);
            _cardObjects.Add(view);
        }

        if (_drawButton != null && _drawButton != _checkNextCardButton)
        {
            _drawButton.gameObject.SetActive(false);
        }

        if (_cardObjects.Count == 0)
        {
            return;
        }

        if (_checkNextCardButton != null)
        {
            _checkNextCardButton.gameObject.SetActive(true);
        }

        _cardObjects[0].gameObject.SetActive(true);
        _cardObjects[0].SwitchStatus(CardStatus.CanFlip);
        _currentCardIndex = 0;
    }

    static CardShaderType ToShaderType(int rarity)
    {
        if (rarity < 0 || rarity > (int)CardShaderType.Outline)
        {
            return CardShaderType.None;
        }

        return (CardShaderType)rarity;
    }

    static async UniTask<Texture> LoadCardTextureAsync(string poolKey, string cardId)
    {
        if (!CardPoolAddress.TryGetBagFolder(poolKey, out string bag) || string.IsNullOrEmpty(cardId))
        {
            return null;
        }

        string path = $"Assets/UI/Card/{bag}/{cardId}.jpg";
        var handle = Addressables.LoadAssetAsync<Sprite>(path);
        try
        {
            Sprite sprite = await handle.Task;
            return sprite != null ? sprite.texture : null;
        }
        catch
        {
            if (handle.IsValid())
            {
                Addressables.Release(handle);
            }

            ALog.LogWarning($"加载卡图失败. Path={path}", ALogCategories.Net);
            return null;
        }
    }

    private void CheckNextCard()
    {
        if (_cardObjects.Count == 0)
        {
            return;
        }

        if(!_cardObjects[_currentCardIndex].IsFlipped())
        {
            _cardObjects[_currentCardIndex].DoFlip();
            return;
        }
        
        int m_index = _currentCardIndex;
        _cardObjects[m_index].DoDissolve(
            () =>
            {
                _cardObjects[m_index].gameObject.SetActive(false);
                _cardObjects[m_index].SwitchStatus(CardStatus.ShowEnd);
            }
        );

        _currentCardIndex++;
        if(_currentCardIndex < _cardObjects.Count)
        {
            _cardObjects[_currentCardIndex].gameObject.SetActive(true);
        }

        for(int i = _currentCardIndex; i < _cardObjects.Count; i++)
        {
            int index = i;
            _cardObjects[index].DoTranslateZ(index - _currentCardIndex, () =>
            {
                _cardObjects[index].SwitchStatus(CardStatus.CanFlip);
            });
        }


        if(_currentCardIndex   == _cardObjects.Count)
        {
            ALog.Log("所有卡牌已翻开", ALogCategories.Default);
            if (_checkNextCardButton != null)
            {
                _checkNextCardButton.gameObject.SetActive(false);
            }

            DoEndShow().Forget();
        }
    }

    private async UniTask DoEndShow()
    {
        await UniTask.Delay(1000);

        for(int i = 0; i < _cardObjects.Count; i++)
        {
            _cardObjects[i].gameObject.SetActive(true);
        }
        if (_gameObjectHorizontalLayout != null)
        {
            _gameObjectHorizontalLayout.enabled = true;
        }

        
        int count = _cardObjects.Count;
        int left = (count - 1) / 2;
        int right = count / 2;

        while (left >= 0)
        {
            await UniTask.Delay(100);
            _cardObjects[left].RestoreDissolve();
            if (right != left)
            {
                _cardObjects[right].RestoreDissolve();
            }

            left--;
            right++;
        }
    }

}
