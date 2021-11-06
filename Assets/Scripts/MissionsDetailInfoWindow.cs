using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MissionsDetailInfoWindow : BaseWindow
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI chanceText;
    [SerializeField] private List<Image> specializationImages;
    [SerializeField] private List<GameObject> addHackerObjects;
    [SerializeField] private List<GameObject> hackerInfoObjects;

    [SerializeField] private TextMeshProUGUI rewardPointsText;
    [SerializeField] private TextMeshProUGUI losePointsText;
    [SerializeField] private TextMeshProUGUI rewardScoreText;
    [SerializeField] private TextMeshProUGUI loseScoreText;

    [SerializeField] private Button startMissionButton;

    private List<Hacker> selectedHackers;
    private MissionData missionData;

    public static Hacker currentHacker;

    protected override void Awake()
    {
        base.Awake();
        EventSystem.OnEnableMissionDetailWindowNeeded += OnEnableMissionDetailWindowNeeded;

        foreach (var item in addHackerObjects)
        {
            item.GetComponentInChildren<Button>().onClick.AddListener(AddHackerButtonOnClick);
        }
        selectedHackers = new List<Hacker>();

        startMissionButton.onClick.AddListener(StartMissionOnClick);
    }

    public void UpdateData(MissionData _missionData)
    {
        missionData = _missionData;
        titleText.text = _missionData.Name;
        UpdateSuccessChanceText();
        for (int i = 0; i < specializationImages.Count; i++)
        {
            if (i < _missionData.Specializations.Count)
                specializationImages[i].sprite = GameManager.Instance.Cache.GetSprite(_missionData.Specializations[i].ToString());
            else
                specializationImages[i].gameObject.SetActive(false);
        }

        rewardPointsText.text = "+" + _missionData.RewardProcent.ToString() + "%";
        losePointsText.text = "+" + _missionData.LoseProcent.ToString() + "%";
        rewardScoreText.text = _missionData.RewardProcent.ToString(); //TODO: create scores
        loseScoreText.text = _missionData.LoseProcent.ToString();
    }

    private int CalculateSuccessChance()
    {
        int chance = missionData.SuccessChance;
        foreach (var hacker in selectedHackers)
        {
            if (hacker.Stats.Specialization.Exists((spec) => missionData.Specializations.Contains(spec)))
                chance += Consts.SPECIALIZED_HACKER_SUCCESS_CHANCE;
            else
                chance += Consts.HACKER_SUCCESS_CHANCE;
        }

        return chance;
    }

    private void AddHackerButtonOnClick()
    {
        if (selectedHackers.Count < GameManager.Instance.PlayerData.HackerInfoData.Count)
        {
            MyHackersWindowScript hackersWindow = GameManager.Instance.InstantiateWindow("MyHackersWindow").GetComponent<MyHackersWindowScript>();
            hackersWindow.HideBusyHackers();
            hackersWindow.HideSelectedHackers(selectedHackers);
            hackersWindow.EnableButtonsAndHideStatus();
            gameObject.SetActive(false);
        }
        //TODO: add something to notificate that there is no hackers available
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        EventSystem.OnEnableMissionDetailWindowNeeded -= OnEnableMissionDetailWindowNeeded;
    }

    private void OnEnableMissionDetailWindowNeeded()
    {
        if (currentHacker != null)
        {
            selectedHackers.Add(currentHacker);
            currentHacker = null;
            UpdateHackersInfo();
        }

        gameObject.SetActive(true);
    }

    private void UpdateHackersInfo()
    {
        for (int i = 0; i < hackerInfoObjects.Count; i++)
        {
            addHackerObjects[i].SetActive(i >= selectedHackers.Count);
            hackerInfoObjects[i].SetActive(i < selectedHackers.Count);

            if (hackerInfoObjects[i].activeSelf)
                hackerInfoObjects[i].GetComponent<HackerForMissionInfo>().SetDataAndInit(this, selectedHackers[i], i);
        }

        UpdateSuccessChanceText();
    }

    public void CancelButtonOnClick(int hackerIndex)
    {
        GameManager.Instance.PlaySound("ButtonClick");

        if (selectedHackers.Count >= hackerIndex)
        {
            selectedHackers.RemoveAt(hackerIndex);
        }
        UpdateHackersInfo();
    }

    private void UpdateSuccessChanceText()
    {
        chanceText.text = $"���� �� ����: {CalculateSuccessChance()}%";
    }

    private void StartMissionOnClick()
    {
        GameManager.Instance.PlaySound("ButtonClick");

        Mission mission = new Mission() { MissionData = missionData, Hackers = selectedHackers, SuccessChance = CalculateSuccessChance() };
        mission.StartMission();
        GameManager.Instance.PlayerData.CurrentMissions.Add(mission);
        HideWindow();
    }
}
