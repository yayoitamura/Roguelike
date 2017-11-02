using UnityEngine;
using System.Collections;

namespace Completed
{
    //SoundManager
	public class SoundManager : MonoBehaviour 
	{
		public AudioSource efxSource;					//エフェクトaudio source の参照 //場面場面で効果音を入れ変える
		public AudioSource musicSource;					//music audio source の参照
        public AudioClip lastStage;
        public static SoundManager instance = null;		//staticにすることでシーンが変わっても変数は保持され、他のスクリプトがSoundManagerから関数を呼び出せる				
		public float lowPitchRange = .95f;              //乱数で生成されるピッチの最低値
		public float highPitchRange = 1.05f;            //乱数で生成されるピッチの最高値


		void Awake ()
		{
			//SoundManagerのinstanceの有無
			if (instance == null)
				//instanceにSooundManager自身の代入
				instance = this;
            // != this(instanceがある場合)
			else if (instance != this)
				//Destroy this, this enforces our singleton pattern so there can only be one instance of SoundManager.
                //これを破棄すると、シングルトンパターンが適用されるため、SoundManagerのインスタンスは1つしか存在しません。
				Destroy (gameObject);
			
            //scene切り替え時にobjectが破棄されない(シーンが切り替わっても音が継続される(切り替わりごとに最初に戻らない))
			DontDestroyOnLoad (gameObject);
		}


		//Player classのCheckIfGameOverで呼ばれる
		public void PlaySingle(AudioClip clip)
		{
            //efxSourceのクリップに渡されたGameOverSoundを代入
			efxSource.clip = clip;
			
			//clipの再生
			efxSource.Play ();
		}


		//params 引数が幾つでもいい　各script, Enemy, Player, Wallから渡された要素分の配列ができる
		//アクションごとの効果音の代入、数パターンからランダムに選択、ピッチ（音の高低）を変更
		public void RandomizeSfx (params AudioClip[] clips)
		{
            //0~渡されたクリップに数（配列の長さ）で乱数を生成　ランダムで渡された複数のclipから選択して->
			int randomIndex = Random.Range(0, clips.Length);

			//lowPitchRange,とhighPitchRangeの間のランダムな数値を->
			float randomPitch = Random.Range(lowPitchRange, highPitchRange);

            //->ピッチ（音程）に代入
			efxSource.pitch = randomPitch;

			//->efxSource.clip に代入
			efxSource.clip = clips[randomIndex];
			
			//Play the clip.
			efxSource.Play();
		}
	}
}
