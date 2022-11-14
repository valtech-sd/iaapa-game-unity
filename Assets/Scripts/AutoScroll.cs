using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AutoScroll : MonoBehaviour {
	// Start is called before the first frame update
	void Start() {
		StartAutoScroll();
	}

	IEnumerator AutoScrollAnim(ScrollRect srollRect, float startPosition, float endPosition, float duration) {
		yield return new WaitForSeconds(0.5f);
		float t0 = 0.0f;
		while (t0 < 1.0f) {
			t0 += Time.deltaTime / duration;
			srollRect.verticalNormalizedPosition = Mathf.Lerp(startPosition, endPosition, t0);
			yield return null;
		}

        StartAutoScrollReverse();


    }

    IEnumerator AutoScrollAnimReverse(ScrollRect srollRect, float startPosition, float endPosition, float duration)
    {
        yield return new WaitForSeconds(0.5f);
        float t0 = 0.0f;
        while (t0 < 1.0f)
        {
            t0 += Time.deltaTime / duration;
            srollRect.verticalNormalizedPosition = Mathf.Lerp(startPosition, endPosition, t0);
            yield return null;
        }

        StartAutoScroll();


    }


    public void StartAutoScroll() {
		StartCoroutine(AutoScrollAnim(GameObject.Find("Scroll View").GetComponent<ScrollRect>(), 1, 0, 50f));
	}

    public void StartAutoScrollReverse()
    {
        StartCoroutine(AutoScrollAnimReverse(GameObject.Find("Scroll View").GetComponent<ScrollRect>(), 0, 1, 50f));
    }


    // Update is called once per frame
    void Update() {

	}
}
