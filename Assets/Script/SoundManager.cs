using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Sound
{
    public string name; //�� �̸�
    public AudioClip clip; //��
}

public class SoundManager : MonoBehaviour
{
    //�̱���. sington. 1��
    static public SoundManager instance;
    #region singleton
    void Awake() //��ü ������ ���� ����
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(this.gameObject); //this ���� ��� ��
    }
    #endregion singleton

    public AudioSource[] audioSourceEffects; //����Ʈ�� �� ���� ���� �� ���� �� �� �����ϱ�!!
    public AudioSource audioSourceBgm; //����� �翬�� �� ���� �ϳ� ������

    public string[] playSoundName;

    public Sound[] effectSounds;
    public Sound[] bgmSounds;

    void Start()
    {
        playSoundName = new string[audioSourceEffects.Length];
    }


    public void PlaySE(string _name)
    {
        for (int i = 0; i < effectSounds.Length; i++)
        {
            if(_name == effectSounds[i].name)
            {
                for (int j = 0; j < audioSourceEffects.Length; j++)
                {
                    if (!audioSourceEffects[j].isPlaying)
                    {
                        playSoundName[j] = effectSounds[i].name; //��� ���� ����� �ҽ��� �̸� ��ġ��Ŵ
                        audioSourceEffects[j].clip = effectSounds[i].clip;
                        audioSourceEffects[j].Play();
                        return; //��� �������ϱ� ���̻� for�� ���� ����Xx
                    }
                }//End For
                Debug.Log("��� ���� AudioSource�� ������Դϴ�.");
                return;
            }//End If
        }//End For
        Debug.Log(_name + "���尡 SoundManager�� ��ϵ��� �ʾҽ��ϴ�.");
    }
    public void StopAllSE()
    {
        for (int i = 0; i < audioSourceEffects.Length; i++)
        {
            audioSourceEffects[i].Stop();
        }
    }
    public void StopSE(string _name)
    {
        for (int i = 0; i < audioSourceEffects.Length; i++)
        {
            if(playSoundName[i] == _name)
            {
                audioSourceEffects[i].Stop();
                return;
            }
        }
        Debug.Log("��� ����" + _name + "���尡 �����ϴ�.");
    }
}