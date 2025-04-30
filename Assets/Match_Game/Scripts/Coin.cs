using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Coin : MonoBehaviour
{
	[SerializeField] private Text textNumber;
	[SerializeField] private GameObject coinOutline;
	//[SerializeField] private Material outlineMaterial;
	//private	Image image;
	private	int	number = 0;
	private Vector3 originSize;
	public bool isClick = false;
	public	int	Number
	{
		set
		{
			number = value;
			textNumber.text = number.ToString();
		}
		get => number;
	}

	public	Vector3	Position => this.transform.localPosition;

	private void Awake()
	{
		originSize = this.transform.localScale;
		coinOutline.SetActive(false);
		//image	= GetComponent<Image>();
	}

	public void OnSelected()
	{
		//image.color = Color.blue;
		//image.material = outlineMaterial;
		SizeUpAnimation(this.gameObject, true);
		coinOutline.SetActive(true);
	}

	public void OnDeselected()
	{
		//image.color = Color.red;
		//image.material = null;
		SizeUpAnimation(this.gameObject, false);
		coinOutline.SetActive(false);
	}

    private void SizeUpAnimation(GameObject title, bool isSizeUp){
		if(isSizeUp){
        	title.transform.DOScale(originSize * 1.2f, 0.5f).SetEase(Ease.InOutQuad);
		}else{
        	title.transform.DOScale(originSize, 0.5f).SetEase(Ease.InOutQuad);
		}
    }
}
