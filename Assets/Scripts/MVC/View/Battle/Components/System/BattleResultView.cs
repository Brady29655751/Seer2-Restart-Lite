using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleResultView : BattleBaseView
{
    private BattleResult result = null;
    [SerializeField] private AudioClip KOSound;
    [SerializeField] private IAnimator KOAnim;
    [SerializeField] private GameObject winContentImage;
    [SerializeField] private GameObject expResultBlockPrefab;
    [SerializeField] private GameObject captureResultBlockPrefab;
    [SerializeField] private GameObject[] resultObject;

    public void ProcessResult(BattleResult result) {
        this.result = result;

        gameObject.SetActive(true);
        ShowResultKO();
    }

    private void ShowResultKO() {
        if (!result.isKO) {
            ShowResultObject();
            return;
        }

        KOAnim.onAnimEndEvent.SetListener(ShowResultObject);
        KOAnim.gameObject.SetActive(true);
        AudioSystem.instance.PlaySound(KOSound, AudioVolumeType.BattleSE);
    }

    private void ShowResultObject() {
        foreach (var obj in resultObject) {
            obj?.SetActive(false);
        }

        if (result.state == BattleResultState.OpEscape) {
            resultObject[0]?.SetActive(true);
        } else if (result.state == BattleResultState.CaptureSuccess) {
            resultObject[1]?.SetActive(true);
            ShowPetCaptureResult(resultObject[1]);
        } else if (result.isMyWin) {
            resultObject[2]?.SetActive(true);
            ShowPetExpResult(resultObject[2]);
        } else {
            resultObject[3]?.SetActive(true);
        }
    }

    private void ShowPetCaptureResult(GameObject captureObject) {
        if ((captureObject == null) || (result.capturePet == null))
            return;

        GameObject obj = Instantiate(captureResultBlockPrefab, captureObject.transform);
        RectTransform rect = obj.GetComponent<RectTransform>();
        PetCaptureResultBlockView captureView = obj.GetComponent<PetCaptureResultBlockView>();

        rect.anchoredPosition = new Vector2(0, -5);
        captureView?.SetPet(result.capturePet);
    }

    private void ShowPetExpResult(GameObject winObject) {
        if (winObject == null)
            return;

        if (result.gainExpPerPet == 0) {
            winContentImage?.SetActive(true);
            return;
        }
        winContentImage?.SetActive(false);

        int count = result.fightPetCursors.Count;
        List<Vector2> posList = GetPetExpResultBlockPosition(count);

        foreach (var id in result.fightPetCursors) {
            GameObject obj = Instantiate(expResultBlockPrefab, winObject.transform);
            RectTransform rect = obj.GetComponent<RectTransform>();
            PetExpResultBlockView expResultBlockView = obj.GetComponent<PetExpResultBlockView>();
            Pet pet = Player.instance.petBag[id];
            Vector2 pos = posList[--count];

            rect.localScale = 0.9f * Vector3.one;
            rect.anchoredPosition = pos;

            expResultBlockView.SetPet(pet);
            expResultBlockView.SetGainExpText(result.gainExpPerPet);
            expResultBlockView.SetLevelUpExpText((pet.level >= 100) ? 0 : pet.levelUpExp);
            expResultBlockView.SetGainEVText(result.gainEVStoragePerPet);
        }
    }

    private List<Vector2> GetPetExpResultBlockPosition(int blockCount) {
        List<Vector2> posList = new List<Vector2>();
        List<int> x = new List<int>() { 255, 128, 0, -128, -255};
        List<int> y = new List<int>() { -10, 40, -60 };

        if (blockCount == 1) {
            posList.Add(new Vector2(x[2], y[0]));
        } else if (blockCount == 2) {
            posList.Add(new Vector2(x[1], y[0]));
            posList.Add(new Vector2(x[3], y[0]));
        } else if (blockCount == 3) {
            posList.Add(new Vector2(x[0], y[0]));
            posList.Add(new Vector2(x[2], y[0]));
            posList.Add(new Vector2(x[4], y[0]));
        } else if (blockCount == 4) {
            posList.Add(new Vector2(x[1], y[1]));
            posList.Add(new Vector2(x[3], y[1]));
            posList.Add(new Vector2(x[1], y[2]));
            posList.Add(new Vector2(x[3], y[2]));
        } else if (blockCount == 5) {
            posList.Add(new Vector2(x[0], y[1]));
            posList.Add(new Vector2(x[2], y[1]));
            posList.Add(new Vector2(x[4], y[1]));
            posList.Add(new Vector2(x[1], y[2]));
            posList.Add(new Vector2(x[3], y[2]));
        } else {
            for (int i = 0; i <= 4; i += 2) {
                for (int j = 1; j <= 2; j++) {
                    posList.Add(new Vector2(x[i], y[j]));
                }
            }
        }
        return posList;
    }
}
