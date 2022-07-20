using SupremacyHangar.Runtime.Environment;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Zenject;
using UnityEngine.UI;

namespace SupremacyHangar
{
    public class PlayerTeleporter : MonoBehaviour
    {
        [SerializeField] private Animator loadingAnim;
        [SerializeField] private TMP_Dropdown teleportList;
        private EnvironmentManager _environmentManager;
        private Button teleportButton;

        [Inject]
        public void Construct(EnvironmentManager environmentManager)
        {
            _environmentManager = environmentManager;
        }

        // Start is called before the first frame update
        public void Start()
        {
            teleportButton = GetComponent<Button>();
            teleportButton.onClick.AddListener(delegate { TeleportPlayer(); });
        }

        private void TeleportPlayer()
        {
            loadingAnim.SetBool("IsLoading", true);

            _environmentManager.TeleportPlayer(teleportList.value, loadingAnim);
        }
    }
}
