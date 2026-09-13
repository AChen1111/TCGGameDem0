using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using Cysharp.Threading.Tasks;

//抽卡界面的卡牌列表控制器
public class CardListController : MonoBehaviour {
    int _cardCount;//每次抽取的数量
    [SerializeField] private GameObjectHorizontalLayout _gameObjectHorizontalLayout;
    [SerializeField] private List<CardViewData> _cardViewDatas = new List<CardViewData>();
    private List<CardView> _cardObjects = new List<CardView>();
    public GameObject _cardPrefab;
    public Button _checkNextCardButton;
    private int _currentCardIndex = 0;

    void Awake()
    {
        _gameObjectHorizontalLayout.enabled = false;
        Init();
    }
    void Init()
    {
        //todo:从服务器上获取这次key的抽取结果
        _cardCount = _cardViewDatas.Count;
        for (int i = 0; i < _cardCount; i++)
        {
            GameObject card = Instantiate(_cardPrefab, transform);
            _cardObjects.Add(card.GetComponent<CardView>());
            _cardObjects[i].Init(_cardViewDatas[i]);
            _cardObjects[i].gameObject.SetActive(false);
            _cardObjects[i].SwitchStatus(CardStatus.None);
            _cardObjects[i].gameObject.transform.localPosition = new Vector3(0, 0, i);//z轴排列
        }

        _checkNextCardButton.onClick.AddListener(CheckNextCard);

        //实例化第一张卡牌
        _cardObjects[0].gameObject.SetActive(true);
        _cardObjects[0].SwitchStatus(CardStatus.CanFlip);
        _currentCardIndex = 0;
    }

    private void CheckNextCard()
    {
        //反面说明还没有翻开
        if(!_cardObjects[_currentCardIndex].IsFlipped())
        {
            _cardObjects[_currentCardIndex].DoFilp();
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

        //给剩余的卡牌回到z的位置
        for(int i = _currentCardIndex; i < _cardObjects.Count; i++)
        {
            int index = i;//解决闭包问题
            _cardObjects[index].DoTranslateZ(index - _currentCardIndex, () =>
            {
                _cardObjects[index].SwitchStatus(CardStatus.CanFlip);
            });
        }


        if(_currentCardIndex   == _cardObjects.Count)
        {
            ALog.Log("所有卡牌已翻开", ALogCategories.Default);
            _checkNextCardButton.gameObject.SetActive(false);
            DoEndShow().Forget();
        }
    }

    private async UniTask DoEndShow()
    {
        await UniTask.Delay(1000);

        //全部显示
        for(int i = 0; i < _cardObjects.Count; i++)
        {
            _cardObjects[i].gameObject.SetActive(true);
        }
        _gameObjectHorizontalLayout.enabled = true;

        
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