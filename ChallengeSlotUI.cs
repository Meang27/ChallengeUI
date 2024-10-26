using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChallengeSlotUI : MonoBehaviour, IPointerEnterHandler
{
    [Header("도전과제 타입에 따른 아이콘 셋팅")]
    [SerializeField] ChallengeIconSlot m_challengeIconSlot;
    [Header("도전과제 제목")]
    [SerializeField] TextMeshProUGUI m_TitleText;
    [Header("도전과제 설명")]
    [SerializeField] TextMeshProUGUI m_ExplainText;

    [Header("진행도 게이지")]
    [SerializeField] Slider m_Slider;
    [Header("진행도 텍스트")]
    [SerializeField] TextMeshProUGUI m_CountText;

    [SerializeField] RewardSlotUI m_RewardSlotUI_Prefab;
    [SerializeField] Transform m_SlotParent;
    [SerializeField] GameObject[] m_SlotBackArr;

    [Header("선택마크")]
    [SerializeField] GameObject m_SelectedMark;
    [Header("보상받기 버튼")]
    [SerializeField] GameObject m_RewardButton;
    [Header("도전과제 최대레벨 도달 완료 및 보상받기 완료했을때 Mark")]
    [SerializeField] GameObject m_CompletedMark;

    #region [Private Variables]
    private int m_UISlotIndex = -1;
    private List<RewardSlotUI> m_RewardSlotUIList = new List<RewardSlotUI>();
    #endregion

    #region [Properties]
    private ChallengeManager.ChallengeUnit m_ChallengeInfo;
    public ChallengeManager.ChallengeUnit ChallengeUnit { get { return m_ChallengeInfo; } }
    #endregion

    public void ActiveControl(bool isShow)
    {
        gameObject.SetActive(isShow);
    }

    public void SettingSlotUI(ChallengeManager.ChallengeUnit eventInfo, int slotIndex, int selectIndex)
    {
        //도전 정보 셋팅
        m_ChallengeInfo = eventInfo;
        //UI 슬롯 인덱스 셋팅
        m_UISlotIndex = slotIndex;

        if (m_RewardSlotUIList == null)
            m_RewardSlotUIList = new List<RewardSlotUI>();

        //원본 프리팹 비활성
        m_RewardSlotUI_Prefab.ActiveControl(false);
        //활성화
        ActiveControl(true);
        //슬롯의 선택 상태 업데이트
        UpdateSelectedMark(selectIndex);        
        //도전과제 Tab에 따른 아이콘 셋팅
        m_challengeIconSlot.SettingChallengeIcon(m_ChallengeInfo.ArchiveSetType);
        //도전과제 제목 및 설명 텍스트 셋팅
        _SettingTitleText();
        //도전과제 진행도 셋팅
        _SettingExpGauge();
        //슬롯의 도전과제 상태 셋팅에 따른 오브젝트 활성화
        UpdateReceiveButtonCompleteMark();
        //도전과제 보상 셋팅
        _SettingRewardSlotUI(m_ChallengeInfo.GetRewardsFromLevel(m_ChallengeInfo.CurrentLevel));
        //도전과제 보상 갯수에 따른 백판 수정
        _SettingRewardBackground();
    }

    /// <summary>
    /// 슬롯의 선택 상태를 업데이트
    /// </summary>
    public void UpdateSelectedMark(int selectedIndex)
    {
        m_SelectedMark.SetActive(m_UISlotIndex == selectedIndex);
    }

    /// <summary>
    /// 보상 받기 및 완료 마크 갱신
    /// </summary>
    public void UpdateReceiveButtonCompleteMark()
    {
        m_CompletedMark.SetActive(m_ChallengeInfo.MissionAccomplished);
        if (m_ChallengeInfo.ArchiveSetType == ChallengeManager.ArchiveSetType.Archivement)
            m_RewardButton.SetActive(m_ChallengeInfo.RewardAvailable);
        else
            m_RewardButton.SetActive(false);
    }

    /// <summary>
    /// 도전과제 제목, 내용 텍스트 셋팅
    /// </summary>
    private void _SettingTitleText()
    {
        ChallengeConstData.ChallengeUnit unitInfo = ChallengeManager.Instance.ConstData.ChallengeTable[m_ChallengeInfo.ID];
        ChallengeConstData.CheckTypeDesc nameDescInfo = ChallengeManager.Instance.ConstData.CheckTypeDescTable[unitInfo.CheckType];
        if (nameDescInfo != null)
        {
            // 업적은 레벨을 보여줌
            if (m_ChallengeInfo.ArchiveSetType == ChallengeManager.ArchiveSetType.Archivement)
            {
                string levelForm = TextManager.Instance.GetText(TextID.LEVEL_FORM);
                string level = string.Empty;

                if (m_ChallengeInfo.CurrentLevel == m_ChallengeInfo.MaxLevel)
                    level = string.Format(levelForm, m_ChallengeInfo.CurrentLevel);
                else
                    level = string.Format(levelForm, m_ChallengeInfo.CurrentLevel + 1);
                m_TitleText.text = string.Format(TextManager.Instance.GetText(TextID.FORM_TWO_HYPHEN), level, TextManager.Instance.GetText(nameDescInfo.NameTextID));
            }
            else
            {
                m_TitleText.text = TextManager.Instance.GetText(nameDescInfo.NameTextID);
            }
            //설명 셋팅
            m_ExplainText.text = TextManager.Instance.GetText(nameDescInfo.DescTextID);
        }
    }

    /// <summary>
    /// 경험치 셋팅
    /// </summary>
    private void _SettingExpGauge()
    {
        m_Slider.value = (float)m_ChallengeInfo.CurrentProgress / (float)m_ChallengeInfo.CurrentGoal.Item2;
        m_CountText.text = string.Format(TextManager.Instance.GetText(TextID.FORM_TWO_VALUE_SLASH), m_ChallengeInfo.CurrentProgress, m_ChallengeInfo.CurrentGoal.Item2);
    }

    /// <summary>
    /// 보상 상품 슬롯 UI 셋팅
    /// </summary>
    private void _SettingRewardSlotUI(List<(GoodsID, int)> reward)
    {
        int iLoopCount = m_RewardSlotUIList.Count > reward.Count ? m_RewardSlotUIList.Count : reward.Count;
        for (int i = 0; i < iLoopCount; i++)
        {
            if (i >= m_RewardSlotUIList.Count)
            {
                if (i >= reward.Count) break;
                RewardSlotUI slotUI = Instantiate(m_RewardSlotUI_Prefab, m_SlotParent);
                slotUI.ShowRewardSlotUI(reward[i].Item1, reward[i].Item2);
                m_RewardSlotUIList.Add(slotUI);
            }
            else
            {
                if (i >= reward.Count)
                {
                    if (i < m_RewardSlotUIList.Count)
                    {
                        m_RewardSlotUIList[i].ActiveControl(false);
                        continue;
                    }
                    break;
                }
                m_RewardSlotUIList[i].ShowRewardSlotUI(reward[i].Item1, reward[i].Item2);
            }
        }
    }

    /// <summary>
    /// 보상 상품 백그라운드 셋팅
    /// </summary>
    private void _SettingRewardBackground()
    {
        for (int i = 0; i < m_SlotBackArr.Length; i++)
        {
            if (i >= m_RewardSlotUIList.Count)
                m_SlotBackArr[i].SetActive(false);
            else
            {
                bool isValid = m_RewardSlotUIList[i].gameObject.activeSelf;
                m_SlotBackArr[i].SetActive(isValid);
            }
        }
    }

    #region [Event Method]
    /// <summary>
    /// 보상받기 버튼 클릭시 호출
    /// </summary>
    public void ClickGetReward()
    {
        if (m_ChallengeInfo.RewardAvailable == false) return;
        int[] challengeIDArr = new int[] { m_ChallengeInfo.ID };
        //이벤트 보내기
        Eventboard.Instance.SendEvent(nameof(ChallengeSlotUI), ChallengeUI.ReceiverName, ChallengeUI.EventType.SendRewardPacket, new object[] { m_UISlotIndex, challengeIDArr });
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SoundManager.Instance.PlayUISound(SoundID.SFX_OVER);
        Eventboard.Instance.SendEvent(nameof(ChallengeSlotUI), ChallengeUI.ReceiverName, ChallengeUI.EventType.SlotSelectUpdate, new object[] { m_UISlotIndex });
    }

    /// <summary>
    /// 모든 보상 정보 보기 이벤트
    /// </summary>
    public void ClickShowTotalReward()
    {
        if (m_ChallengeInfo == null) return;
        if (m_ChallengeInfo.ArchiveSetType != ChallengeManager.ArchiveSetType.Archivement) return;
        Eventboard.Instance.SendEvent(nameof(ChallengeSlotUI), ChallengeUI.ReceiverName, ChallengeUI.EventType.ShowTotalReward);
    }
    #endregion
}
