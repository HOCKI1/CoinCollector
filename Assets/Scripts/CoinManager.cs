using AxGrid.Base;
using TMPro;
using UnityEngine;
using AxGrid.Model;


public class CoinManager : MonoBehaviourExtBind
{

    [SerializeField] private TextMeshProUGUI coinsAmountText;
    [SerializeField] private TextMeshProUGUI coinsPlusHunderedText;
    [SerializeField] private TextMeshProUGUI everySecondCoinsAmountText;

    // Инициализация модели 
    [OnStart]
    public void Init()
    {
        Model.Set("Coin", 0);
    }
    
    // Добавление монет
    public void AddCoin()
    {
        Model.Inc("Coin", 1);
    }

    // Обновление текста
    [Bind("OnCoinChanged")]
    public void OnAOrBChanged()
    {
        var coins = Model.Get<int>("Coin");
        coinsAmountText.text = coins.ToString();
        int coinsPlusHundred = coins != 0 ? coins * 100 : 0;
        coinsPlusHunderedText.text = coinsPlusHundred.ToString();
        int everySecondCoin = coins / 2;
        everySecondCoinsAmountText.text = everySecondCoin.ToString();
    }
}
