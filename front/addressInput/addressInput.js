class AddressInput extends HTMLElement {
  constructor() {
    super();
    let shadowRoot=this.attachShadow({ mode: "open" });

    fetch("addressInput/addressInput.html")
      .then(res => res.text())
      .then(data => {
        let template = new DOMParser().parseFromString(data, "text/html")
          .querySelector("template").content;
        this.shadowRoot.appendChild(template.cloneNode(true));

        this.labelEl = this.shadowRoot.querySelector("label");
        this.inputEl = this.shadowRoot.querySelector("input");
        this.suggestionsEl = this.shadowRoot.querySelector("ul");

        this.labelEl.textContent = this.getAttribute("label") || "Address";

        this.inputEl.addEventListener("input", () => this.fetchSuggestions());
      });
  }

  async fetchSuggestions() {
    const query = this.inputEl.value.trim();

    if (query.length < 3) {
      this.suggestionsEl.innerHTML = "";
      return;
    }

    const url = `https://api-adresse.data.gouv.fr/search/?q=${encodeURIComponent(query)}&limit=5`;

    try {
      const res = await fetch(url);
      const data = await res.json();
      this.suggestionsEl.innerHTML = "";

      data.features.forEach(feature => {
        const li = document.createElement("li");
        li.textContent = feature.properties.label;
        li.addEventListener("click", () => {
          this.inputEl.value = feature.properties.label;
          this.suggestionsEl.innerHTML = "";

          this.dispatchEvent(new CustomEvent("address-selected", {
            detail: {
              label: feature.properties.label,
              coordinates: feature.geometry.coordinates
            },
            bubbles: true,
            composed: true
          }));
        });
        this.suggestionsEl.appendChild(li);
      });
    } catch (err) {
      console.error("Error fetching address suggestions:", err);
    }
  }
}

customElements.define("address-input", AddressInput);
