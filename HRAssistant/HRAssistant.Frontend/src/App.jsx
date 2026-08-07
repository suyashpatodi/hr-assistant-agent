import Home from "./components/Home"
import Login from "./components/Login"
import { BrowserRouter, Routes, Route } from "react-router-dom"

const App =() =>{
    return(
       <BrowserRouter>
        <Routes>
            <Route path="/" element={<Login/>}/>
            <Route path="/login" element={<Login/>}/>
        </Routes>
       </BrowserRouter>
    )
}

export default App