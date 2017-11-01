using UnityEngine;
using System.Collections;

namespace Completed
{
    //Wall
    public class Wall : MonoBehaviour
    {
        public AudioClip chopSound1;                //壁がPlayeに攻撃された時のオーディオクリッップ１
        public AudioClip chopSound2;                //壁がPlayeに攻撃された時のオーディオクリッップ２
        public Sprite dmgSprite;                    //Awall破壊された時の差し換え用スプライト
        public int hp = 3;                          //wallのhp
        public GameObject[] items;

        private SpriteRenderer spriteRenderer;      //Store a component reference to the attached SpriteRenderer.
                                                    //付属のSpriteRendererにコンポーネント参照を格納します。 ?

        void Awake()
        {
            //SpriteRendererへのコンポーネント参照を取得します。
            spriteRenderer = GetComponent<SpriteRenderer>();
        }


        //Playerがwallを攻撃した時　Player.OnCantMove で呼ばれる
        public void DamageWall(int loss)
        {

            //SoundManagerのRandomizeSfx関数に二つのオーディオを渡す
            SoundManager.instance.RandomizeSfx(chopSound1, chopSound2);

            //スプライトレンダラーを破損したウォールスプライトに設定します。
            spriteRenderer.sprite = dmgSprite;

            hp -= loss;

            //hpが0以下
            if (hp <= 0)
            {
                //SetActive(false)でgameObjectを無効(非アクティブ)
                gameObject.SetActive(false);
            }

            //壁を壊すとアイテム
            //壁が壊れるのとアイテム出現、取得が同時なので修正
            if (!gameObject.activeSelf)
            {
                GameObject randumItem = items[Random.Range(0, items.Length)];
                Instantiate(randumItem, transform.position, Quaternion.identity);
            }
        }
    }
}
