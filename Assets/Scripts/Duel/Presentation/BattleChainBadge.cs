using System.Collections.Generic;
using UnityEngine;

namespace AChen.Duel.Presentation
{
    public sealed class BattleChainBadge : MonoBehaviour
    {
        [SerializeField] SpriteRenderer m_digitPrefab;
        [SerializeField] Sprite[] m_digits;
        [SerializeField] Transform m_ring;
        readonly List<SpriteRenderer> m_numbers = new List<SpriteRenderer>();
        public void Bind(int number)
        {
            string value = number.ToString();
            for (int i = 0; i < value.Length; i++)
            {
                var digit = Instantiate(m_digitPrefab, transform); digit.gameObject.SetActive(true);
                digit.sprite = m_digits[value[i] - '0'];
                var properties = new MaterialPropertyBlock(); properties.SetTexture("_BaseMap", digit.sprite.texture); digit.SetPropertyBlock(properties);
                float height = 2.5f; digit.transform.localScale = Vector3.one * height / digit.sprite.bounds.size.y;
                digit.transform.localPosition = new Vector3((i - (value.Length - 1) * .5f) * 1.5f, 0, -.02f);
                m_numbers.Add(digit);
            }
        }
        public void Emphasize(bool negated)
        {
            m_ring.localScale *= 1.15f;
            foreach (var digit in m_numbers) digit.color = negated ? new Color(.6f, .6f, .6f) : new Color(1, .86f, .25f);
        }
    }
}
