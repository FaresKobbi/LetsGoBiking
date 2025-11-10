class AppHeader extends HTMLElement {
  constructor() {
    super();
    let shadowRoot= this.attachShadow({ mode: "open" });
    let response = fetch("headerComponent/headerComponent.html")
      .then(res=>res.text())
      .then(data=>{
        let template = new DOMParser().parseFromString(data,'text/html')
          .querySelector("template").content;
        this.shadowRoot.appendChild(template.cloneNode(true))
      })
  }
}

customElements.define("app-header", AppHeader);
