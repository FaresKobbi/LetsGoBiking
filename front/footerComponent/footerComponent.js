class AppFooter extends HTMLElement {
  constructor() {
    super();
    let shadowRoot = this.attachShadow({ mode: "open" });
    let response = fetch("footerComponent/footerComponent.html")
      .then(res=>res.text())
      .then(data =>{
        let template =  new DOMParser().parseFromString(data, "text/html")
          .querySelector("template").content;
        this.shadowRoot.appendChild(template.cloneNode(true));
      })
  }
}

customElements.define("app-footer", AppFooter);
