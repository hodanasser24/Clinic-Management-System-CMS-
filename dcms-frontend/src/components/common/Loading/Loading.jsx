import "./Loading.css";

function Loading({ text = "Loading data..." }) {
  return (
    <div className="loading-box">
      <div className="loader"></div>
      <p>{text}</p>
    </div>
  );
}

export default Loading;
