using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

/// <summary>抽卡展示效果. 只播翻面、溶解和展开, 不持有 UI 按键.</summary>
public class CardPickController : MonoBehaviour
{
    [SerializeField] GameObjectHorizontalLayout _gameObjectHorizontalLayout;
    [SerializeField] GameObject _cardPrefab;
    [SerializeField] Camera _pickCamera;

    readonly List<CardPickView> _cardObjects = new List<CardPickView>();
    ICardPickPointer _pointer;
    int _currentCardIndex;
    bool _revealFinished;
    Action _onFinished;

    public event Action<bool> NextVisibleChanged;

    public Camera PickCamera =>
        _pickCamera != null ? _pickCamera : GetComponentInChildren<Camera>(true);

    public void BindPointer(ICardPickPointer pointer)
    {
        _pointer = pointer;
    }

    public bool TryGetPointerRay(out Ray ray)
    {
        if (_pointer == null)
        {
            ray = default;
            return false;
        }

        return _pointer.TryGetPointerRay(out ray);
    }

    public void Play(IReadOnlyList<CardPickViewData> cards, Action onFinished)
    {
        Clear();
        _onFinished = onFinished;
        if (_gameObjectHorizontalLayout != null)
        {
            _gameObjectHorizontalLayout.enabled = false;
        }

        if (cards == null || cards.Count == 0 || _cardPrefab == null)
        {
            ALog.LogWarning("抽卡效果无卡可播", ALogCategories.UI);
            return;
        }

        int layer = gameObject.layer;
        for (int i = 0; i < cards.Count; i++)
        {
            GameObject card = Instantiate(_cardPrefab, transform);
            SetLayerRecursively(card, layer);
            CardPickView view = card.GetComponent<CardPickView>();
            view.Init(cards[i]);
            view.gameObject.SetActive(false);
            view.SwitchStatus(CardStatus.None);
            view.gameObject.transform.localPosition = new Vector3(0, 0, i);
            _cardObjects.Add(view);
        }

        RaiseNextVisible(true);
        _cardObjects[0].gameObject.SetActive(true);
        _cardObjects[0].SwitchStatus(CardStatus.CanFlip);
        _currentCardIndex = 0;
        ALog.Log($"抽卡效果开始. Count={_cardObjects.Count}", ALogCategories.UI);
    }

    public void Clear()
    {
        for (int i = 0; i < _cardObjects.Count; i++)
        {
            if (_cardObjects[i] != null)
            {
                Destroy(_cardObjects[i].gameObject);
            }
        }

        _cardObjects.Clear();
        _currentCardIndex = 0;
        _revealFinished = false;
        _onFinished = null;
        if (_gameObjectHorizontalLayout != null)
        {
            _gameObjectHorizontalLayout.enabled = false;
        }
    }

    public void Advance()
    {
        if (_revealFinished)
        {
            _onFinished?.Invoke();
            return;
        }

        if (_cardObjects.Count == 0 || _currentCardIndex >= _cardObjects.Count)
        {
            return;
        }

        CheckNextCard();
    }

    public static CardShaderType ToShaderType(int rarity)
    {
        if (rarity < 0 || rarity > (int)CardShaderType.Outline)
        {
            return CardShaderType.None;
        }

        return (CardShaderType)rarity;
    }

    void CheckNextCard()
    {
        if (!_cardObjects[_currentCardIndex].IsFlipped())
        {
            _cardObjects[_currentCardIndex].DoFlip();
            return;
        }

        int dissolveIndex = _currentCardIndex;
        _cardObjects[dissolveIndex].DoDissolve(
            () =>
            {
                if (_cardObjects.Count <= dissolveIndex || _cardObjects[dissolveIndex] == null)
                {
                    return;
                }

                _cardObjects[dissolveIndex].gameObject.SetActive(false);
                _cardObjects[dissolveIndex].SwitchStatus(CardStatus.ShowEnd);
            });

        _currentCardIndex++;
        if (_currentCardIndex < _cardObjects.Count)
        {
            _cardObjects[_currentCardIndex].gameObject.SetActive(true);
        }

        for (int i = _currentCardIndex; i < _cardObjects.Count; i++)
        {
            int index = i;
            _cardObjects[index].DoTranslateZ(index - _currentCardIndex, () =>
            {
                _cardObjects[index].SwitchStatus(CardStatus.CanFlip);
            });
        }

        if (_currentCardIndex == _cardObjects.Count)
        {
            ALog.Log("所有卡牌已翻开", ALogCategories.Default);
            RaiseNextVisible(false);
            DoEndShow().Forget();
        }
    }

    async UniTask DoEndShow()
    {
        await UniTask.Delay(1000);
        if (_cardObjects.Count == 0)
        {
            return;
        }

        for (int i = 0; i < _cardObjects.Count; i++)
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
            if (_cardObjects.Count == 0)
            {
                return;
            }

            _cardObjects[left].RestoreDissolve();
            if (right != left)
            {
                _cardObjects[right].RestoreDissolve();
            }

            left--;
            right++;
        }

        _revealFinished = true;
        RaiseNextVisible(true);
    }

    void RaiseNextVisible(bool visible)
    {
        NextVisibleChanged?.Invoke(visible);
    }

    static void SetLayerRecursively(GameObject root, int layer)
    {
        root.layer = layer;
        Transform transform = root.transform;
        for (int i = 0; i < transform.childCount; i++)
        {
            SetLayerRecursively(transform.GetChild(i).gameObject, layer);
        }
    }
}
